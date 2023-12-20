/*
 * A Simple Fluid Solver Wind by Jos Stam for OpenSim
 *      http://www.dgp.utoronto.ca/people/stam/reality/Research/pub.html
 *
 *
 * Using FFTW v2.0  http://www.fftw.org
 * 
 */


#include <stdlib.h>
#include <string.h>
#include <unistd.h>
#include <math.h>

#include <rfftw.h>


#define MAX_THREAD_NUM  36


static rfftwnd_plan p_plan_rc[MAX_THREAD_NUM];
static rfftwnd_plan p_plan_cr[MAX_THREAD_NUM];

static fftw_real* p_u0[MAX_THREAD_NUM];
static fftw_real* p_v0[MAX_THREAD_NUM];


static int thread_id   = -1;
static int thread_lock = 0;



#define floor(x) ((x)>=0.0 ? ((int)(x)) : (-((int)(1-(x)))))



int init_SFSW(int n)
{
	while (thread_lock) sleep(1);
	thread_lock = 1;

	thread_id++;
	//
	if (thread_id>=MAX_THREAD_NUM) {
		fprintf(stderr, "SFSW: Threads number is over!!\n");
		fflush(stderr);
		thread_lock = 0;
		return -1;
	}

	//
	p_plan_rc[thread_id] = rfftw2d_create_plan(n, n, FFTW_REAL_TO_COMPLEX, FFTW_IN_PLACE);
	p_plan_cr[thread_id] = rfftw2d_create_plan(n, n, FFTW_COMPLEX_TO_REAL, FFTW_IN_PLACE);

	p_u0[thread_id] = (fftw_real*)malloc(sizeof(fftw_real)*n*(n+2));
	p_v0[thread_id] = (fftw_real*)malloc(sizeof(fftw_real)*n*(n+2));

	thread_lock = 0;
	return thread_id;
}


void free_SFSW(int thid)
{
	rfftwnd_destroy_plan(p_plan_rc[thid]);
	rfftwnd_destroy_plan(p_plan_cr[thid]);
	//
	if (p_u0[thid]!=NULL) free(p_u0[thid]);
	if (p_v0[thid]!=NULL) free(p_v0[thid]);
}


void solve_SFSW(int n, float* u, float* v, float* fu, float* fv, int rsize, float visc, float dt, int thid)
{
	fftw_real x, y, f, r_sq, U[2], V[2], s, t; 
	int  i, j, i0, j0, i1, j1;

	fftw_real* u0 = p_u0[thid];
	fftw_real* v0 = p_v0[thid];
	rfftwnd_plan plan_rc = p_plan_rc[thid];
	rfftwnd_plan plan_cr = p_plan_cr[thid];

	//
	for (i=0; i<n*n; i++) {
		u [i] += dt*fu[i]; 		// 速度の変化
		v [i] += dt*fv[i];
		u0[i]  = (fftw_real)u[i]/rsize;
		v0[i]  = (fftw_real)v[i]/rsize;
	}
	
	// advection step (-(u.G).u)
	for (j=0; j<n; j++) {
		for (i=0; i<n; i++) {
			x = i - dt*u0[i+n*j]*n; 
			y = j - dt*v0[i+n*j]*n;

			i0 = floor(x); 
			j0 = floor(y);
			s  = x - i0;		// 小数点以下
			t  = y - j0;

			i0 = (n + (i0%n))%n;
			i1 = (i0 + 1)%n;

			j0 = (n + (j0%n))%n; 
			j1 = (j0 + 1)%n;

			// 線型補間
			u[i+n*j] = (float)((1-s)*((1-t)*u0[i0+n*j0]+t*u0[i0+n*j1]) + s*((1-t)*u0[i1+n*j0]+t*u0[i1+n*j1]));
			v[i+n*j] = (float)((1-s)*((1-t)*v0[i0+n*j0]+t*v0[i0+n*j1]) + s*((1-t)*v0[i1+n*j0]+t*v0[i1+n*j1]));
		}
	}

	for (j=0; j<n; j++) {
		for (i=0; i<n; i++) {
			u0[i+(n+2)*j] = (fftw_real)u[i+n*j]; 
			v0[i+(n+2)*j] = (fftw_real)v[i+n*j];
		}
	}
	
	rfftwnd_one_real_to_complex(plan_rc, u0, (fftw_complex*)u0);
	rfftwnd_one_real_to_complex(plan_rc, v0, (fftw_complex*)v0);

	for (j=0; j<n; j++) {
		y = j<=n/2 ? j : j-n;
		//
		for (i=0; i<=n; i+=2) {
			x = 0.5*i;
			r_sq = x*x + y*y;
			if (r_sq==0.0) continue;
			f = exp(-r_sq*dt*visc);

			U[0] = u0[i  +(n+2)*j]; V[0] = v0[i  +(n+2)*j];
			U[1] = u0[i+1+(n+2)*j]; V[1] = v0[i+1+(n+2)*j];

			u0[i  +(n+2)*j] = f*((1-x*x/r_sq)*U[0]     -x*y/r_sq *V[0]);
			u0[i+1+(n+2)*j] = f*((1-x*x/r_sq)*U[1]     -x*y/r_sq *V[1]);
			v0[i+  (n+2)*j] = f*(  -y*x/r_sq *U[0] + (1-y*y/r_sq)*V[0]);
			v0[i+1+(n+2)*j] = f*(  -y*x/r_sq *U[1] + (1-y*y/r_sq)*V[1]);
		}
	}
	
	rfftwnd_one_complex_to_real(plan_cr, (fftw_complex*)u0, u0);
	rfftwnd_one_complex_to_real(plan_cr, (fftw_complex*)v0, v0);
	
	f = 1.0/(n*n);
	for (j=0; j<n; j++) {
		for (i=0; i<n; i++) {
			u[i+n*j] = (float)(f*u0[i+(n+2)*j])*rsize; 
			v[i+n*j] = (float)(f*v0[i+(n+2)*j])*rsize; 
		}
	}
}



/*
// for TEST
int main()
{
	int n = 16;

	int id = init_SFSW(n);
	
	float* u = (float*)malloc(sizeof(float)*n*n);
	float* v = (float*)malloc(sizeof(float)*n*n);

	memset(u, 0, sizeof(float)*n*n);
	memset(v, 0, sizeof(float)*n*n);

	float* fu = (float*)malloc(sizeof(float)*n*n);
	float* fv = (float*)malloc(sizeof(float)*n*n);

	int i;

	for (i=0; i<n*n; i++) {
		fu[i] = 1.0;
		fv[i] = 0.5;
	}
	memset(u, 0, sizeof(float)*n*n);
	memset(v, 0, sizeof(float)*n*n);

	int rsize = 256;

	solve_SFSW(16, u, v, fu, fv, rsize, 0.001, 1.0, id);
	printf("A = %f %f\n", u[0], v[0]);

	solve_SFSW(16, u, v, fu, fv, rsize, 0.001, 1.0, id);
	printf("A = %f %f\n", u[0], v[0]);

	solve_SFSW(16, u, v, fu, fv, rsize, 0.001, 1.0, id);
	printf("A = %f %f\n", u[0], v[0]);

	free_SFSW(id);

	return 0;
}
*/
