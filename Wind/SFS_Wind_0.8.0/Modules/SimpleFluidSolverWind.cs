//
// A Simple Fluid Solver Wind Module for OpenSim
//	
// 		Using AForge.NET for FFT
//
// 		by Fumi.Iseki
//

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Security;
using System.Runtime.InteropServices;

using log4net;
using OpenMetaverse;
using Mono.Addins;

using OpenSim.Framework;
using OpenSim.Region.Framework.Interfaces;
using OpenSim.Region.CoreModules.World.Wind;


[assembly: Addin("SimpleFluidSolverWind", "0.1")]
[assembly: AddinDependency("OpenSim", "0.5")]


namespace OpenSim.Region.CoreModules.World.Wind.Plugins
{
	[Extension(Path = "/OpenSim/WindModule", NodeName = "WindModel", Id = "SimpleFluidSolverWind")]
	class SimpleFluidSolverWind : Mono.Addins.TypeExtensionNode, IWindModelPlugin
	{
		private const int   m_mesh = 16;
		//
		private int   m_extr_force = 0;				// Kind of the external force
		private float m_damping_rate = 0.85f;		// Damping rate of the externl force
		private float m_viscosity  = 0.001f;		// Viscosity coefficient of the wind
		private int   m_region_size = 256;

		private float m_energy_eps = 0.004f;		// Lower limit of the energy variation rate
		private float m_energy = 0.0f;
		private int   m_energy_cnt = 0;

		private int   m_period = 0;					// Period of external the external force
		private int   m_period_cnt = 0;

		private static readonly ILog m_log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private Vector2[] m_windSpeeds = null;

		private float m_strength = 1.0f;
		private Random m_rndnums = null;

		private float[] m_windSpeeds_u = null;
		private float[] m_windSpeeds_v = null;
		private float[] m_windForces_u = null;
		private float[] m_windForces_v = null;
		private float[] m_extrForces_u = null;
		private float[] m_extrForces_v = null;

		private float[] m_work_u = null;
		private float[] m_work_v = null;

		private AForge.Math.Complex[,] m_comp_u = null;
		private AForge.Math.Complex[,] m_comp_v = null;


		#region IPlugin Members

		public string Version
		{
			get { return "1.0.0.0"; }
		}

		public string Name
		{
			get { return "SimpleFluidSolverWind"; }
		}


		public void Initialise()
		{
			m_rndnums = new Random(Environment.TickCount);

			m_windSpeeds_u = new float[m_mesh*m_mesh];
			m_windSpeeds_v = new float[m_mesh*m_mesh];
			m_windForces_u = new float[m_mesh*m_mesh];
			m_windForces_v = new float[m_mesh*m_mesh];
			m_extrForces_u = new float[m_mesh*m_mesh];
			m_extrForces_v = new float[m_mesh*m_mesh];
			//
			m_windSpeeds   = new Vector2[m_mesh*m_mesh];

			m_work_u = new float[m_mesh*m_mesh];
			m_work_v = new float[m_mesh*m_mesh];

			m_comp_u = new AForge.Math.Complex[m_mesh, m_mesh];
			m_comp_v = new AForge.Math.Complex[m_mesh, m_mesh];

			clearForces();
			clearSpeeds();
			addForces(m_extr_force);
		}

		#endregion


		#region IDisposable Members

		public void Dispose()
		{
			m_windSpeeds   = null;
			//
			m_windSpeeds_u = null;
			m_windSpeeds_v = null;
			m_windForces_u = null;
			m_windForces_v = null;
			m_extrForces_u = null;
			m_extrForces_v = null;

			m_work_u = null;
			m_work_v = null;
			//
			m_comp_u = null;
			m_comp_v = null;
		}

		#endregion


		#region IWindModelPlugin Members

		public void WindConfig(OpenSim.Region.Framework.Scenes.Scene scene, Nini.Config.IConfig windConfig)
		{
			if (windConfig != null)
			{
				if (windConfig.Contains("strength"))
				{
					m_strength = windConfig.GetFloat("strength", 1.0f);
				}
				if (windConfig.Contains("damping"))
				{
					m_damping_rate = windConfig.GetFloat("damping", 1.0f);
					if (m_damping_rate>1.0f) m_damping_rate = 1.0f;
				}
				if (windConfig.Contains("force"))
				{
					m_extr_force = windConfig.GetInt("force", 0);
					if (m_extr_force<0) m_extr_force = 0;
				}
				if (windConfig.Contains("period"))
				{
					m_period = windConfig.GetInt("period", 0);
					if (m_period<0) m_period = 0;
				}
				if (windConfig.Contains("wind_eps"))
				{
					m_energy_eps = windConfig.GetFloat("wind_eps", 0.001f);
					if (m_energy_eps<=0.0f) m_energy_eps = 0.001f;
				}
				if (windConfig.Contains("wind_visc"))
				{
					m_viscosity = windConfig.GetFloat("wind_visc", 0.001f);
					if (m_viscosity<0.0f) m_viscosity = 0.001f;
				}
				//
				if (windConfig.Contains("region"))
				{
					m_region_size = windConfig.GetInt("region", 256);
					m_region_size = (((int)Math.Abs(m_region_size)+255)/256)*256;
					if (m_region_size==0) m_region_size = 256;
				}
				else if (scene!=null)
				{
					m_region_size = (int)scene.RegionInfo.RegionSizeX;
				}
			}
		}


		public void WindUpdate(uint frame)
		{
			if (m_windSpeeds!=null)
			{
				for (int i=0; i<m_mesh*m_mesh; i++) {
					m_windForces_u[i] = m_extrForces_u[i]*m_strength;
					m_windForces_v[i] = m_extrForces_v[i]*m_strength;
				}

				SolveSFSW(m_mesh, m_windSpeeds_u, m_windSpeeds_v, m_windForces_u, m_windForces_v, m_region_size, m_viscosity, 1.0f);
				//
				//int center = m_mesh*m_mesh/2;
				//m_log.InfoFormat("[SFSW]: V ==> ({0} {1}) ({2} {3})", m_windSpeeds_u[0], m_windSpeeds_v[0], m_windSpeeds_u[center], m_windSpeeds_v[center]);
				//m_log.InfoFormat("[SFSW]: F ==> ({0} {1}) ({2} {3})", m_extrForces_u[0], m_extrForces_v[0], m_extrForces_u[center], m_extrForces_v[center]);

				float energy = 0.0f;
				for (int i=0; i<m_mesh*m_mesh; i++)
				{
					m_windSpeeds[i].X = m_windSpeeds_u[i];
					m_windSpeeds[i].Y = m_windSpeeds_v[i];
					//
					m_extrForces_u[i] *= m_damping_rate;
					m_extrForces_v[i] *= m_damping_rate;

					energy += m_windSpeeds_u[i]*m_windSpeeds_u[i] + m_windSpeeds_v[i]*m_windSpeeds_v[i];
				}
				//
				if (energy!=0.0f)
				{
					float st_rate = (m_energy-energy)/energy;
					//m_log.InfoFormat("[SFSW]: E ==> {0} {1}", energy, st_rate);
					//
					if (st_rate>=0.0f && st_rate<=m_energy_eps) 
					{
						m_energy_cnt++;
						if (m_energy_cnt>5)
						{
				  			m_log.InfoFormat("[SFSW]: Restart Wind by Energy Limit.");
							// restart wind
				  			clearForces();
				  			clearSpeeds();
							addForces(m_extr_force);
							m_period_cnt = 0;
							m_energy_cnt = 0;
						}
					}
					else m_energy_cnt = 0;
				}
				else m_energy_cnt = 0;

				m_energy = energy;
				//
				if (m_period!=0)
				{
					m_period_cnt++;
					if (m_period_cnt>=m_period)
					{
				  		m_log.InfoFormat("[SFSW]: Restart Wind by Priod.");
						// restart wind
				  		clearForces();
				  		clearSpeeds();
						addForces(m_extr_force);
						m_period_cnt = 0;
						m_energy_cnt = 0;
					}
				}
			}
		}


		public Vector3 WindSpeed(float fX, float fY, float fZ)
		{
			Vector3 windVector = new Vector3(0.0f, 0.0f, 0.0f);

			if (m_windSpeeds!=null)
			{
				int x = (int)fX/m_mesh;
				int y = (int)fY/m_mesh;

				if (x<0) 		x = 0;
				if (x>m_mesh-1) x = m_mesh - 1;
				if (y<0) 		y = 0;
				if (y>m_mesh-1) y = m_mesh - 1;

				windVector.X = m_windSpeeds[y*m_mesh + x].X;
				windVector.Y = m_windSpeeds[y*m_mesh + x].Y;
			}

			return windVector;
		}


		public Vector2[] WindLLClientArray()
		{
			return m_windSpeeds;
		}


		public string Description
		{
			get 
			{
				return "Provides a simple fluid solver wind by Jos Stam."; 
			}
		}


		public System.Collections.Generic.Dictionary<string, string> WindParams()
		{
			Dictionary<string, string> Params = new Dictionary<string, string>();

			Params.Add("force", "Kind of the external force");
			Params.Add("period", "Period of the external force");
			Params.Add("strength", "Wind strength");
			Params.Add("damping", "Damping rate of the external force");
			Params.Add("wind_visc", "Viscosity coefficient of the wind");
			Params.Add("wind_eps", "Lower limit of the energy variation rate");
			Params.Add("region", "Size of the region");
			Params.Add("stop", "Stop the wind");

			return Params;
		}


		public void WindParamSet(string param, float value)
		{
			switch(param)
			{
				case "force":
				  m_extr_force = (int)value;
				  if (m_extr_force<0) m_extr_force = 0;
				  m_log.InfoFormat("[SFSW]: Set Param : force = {0}", m_extr_force);
				  clearForces();
				  addForces(m_extr_force);
				  break;

				case "strength":
				  m_strength = value;
				  m_log.InfoFormat("[SFSW]: Set Param : strength = {0}", m_strength);
				  break;

				case "damping":
				  m_damping_rate = value;
				  if (m_damping_rate>1.0f) m_damping_rate = 1.0f;
				  m_log.InfoFormat("[SFSW]: Set Param : damping = {0}", m_damping_rate);
				  break;

				case "period":
				  m_period = (int)value;
				  if (m_period<0) m_period = 0;
				  m_log.InfoFormat("[SFSW]: Set Param : period = {0}", m_period);
				  m_period_cnt = 0;
				  break;

				case "wind_visc":
				  m_viscosity = value;
				  if (m_viscosity<0.0f) m_viscosity = 0.001f;
				  m_log.InfoFormat("[SFSW]: Set Param : wind_visc = {0}", m_viscosity);
				  break;

				case "wind_eps":
				  m_energy_eps = value;
				  if (m_energy_eps<=0.0f) m_energy_eps = 0.001f;
				  m_log.InfoFormat("[SFSW]: Set Param : wind_eps = {0}", m_energy_eps);
				  m_energy_cnt = 0;
				  break;

				case "region":
				  m_region_size = (((int)Math.Abs(value)+255)/256)*256;
				  if (m_region_size==0) m_region_size = 256;
				  m_log.InfoFormat("[SFSW]: Set Param : region = {0}", m_region_size);
				  break;

				case "stop":
				  m_log.InfoFormat("[SFSW]: Command : stop");
				  clearForces();
				  clearSpeeds();
				  break;
			}
		}


		public float WindParamGet(string param)
		{
			switch (param)
			{
				case "force":
				  return (float)m_extr_force;

				case "strength":
				  return m_strength;

				case "damping":
				  return m_damping_rate;

				case "period":
				  return (float)m_period;

				case "wind_visc":
				  return m_viscosity;

				case "wind_eps":
				  return m_energy_eps;

				case "region":
				  return (float)m_region_size;

				default:
				  throw new Exception(String.Format("Unknown {0} parameter {1}", this.Name, param));
			}
		}


		#endregion


		public void clearSpeeds()
		{
			if (m_windSpeeds!=null)
			{
				for (int i=0; i<m_mesh*m_mesh; i++)
				{
					m_windSpeeds[i].X = 0.0f;
					m_windSpeeds[i].Y = 0.0f;
				}
			}

			if (m_windSpeeds_u!=null && m_windSpeeds_v!=null)
			{
				for (int i=0; i<m_mesh*m_mesh; i++)
				{
					m_windSpeeds_u[i] = 0.0f;
					m_windSpeeds_v[i] = 0.0f;
				}
			}
		}


		public void clearForces()
		{
			if (m_extrForces_u!=null && m_extrForces_v!=null)
			{
				for (int i=0; i<m_mesh*m_mesh; i++)
				{
					m_extrForces_u[i] = 0.0f; 
					m_extrForces_v[i] = 0.0f; 
				}
			}
		}


		private void addForces(int force)
		{
			if (m_extrForces_u!=null && m_extrForces_v!=null)
			{
				int i, j;

				// Random
				if (force==0)
				{
					for (i=0; i<m_mesh*m_mesh; i++)
					{
						m_extrForces_u[i] = (float)(m_rndnums.NextDouble()*2d - 1d); // -1 to 1 
						m_extrForces_v[i] = (float)(m_rndnums.NextDouble()*2d - 1d); // -1 to 1 
					}
				}

				// North
				else if (force==1) 
				{
					for (i=m_mesh/3; i<m_mesh-m_mesh/3; i++)
					{
						m_extrForces_v[i+(m_mesh-2)*m_mesh] -= 2.0f;
					}
				}
	
				// East
				else if (force==2) 
				{
					for (j=m_mesh/3; j<m_mesh-m_mesh/3; j++)
					{
						m_extrForces_u[m_mesh - 2 + j*m_mesh] -= 2.0f;
					}
				}

				// South
				else if (force==3) 
				{
					for (i=m_mesh/3; i<m_mesh-m_mesh/3; i++)
					{
						m_extrForces_v[i + m_mesh] += 2.0f;
					}
				}
	
				// West
				else if (force==4) 
				{
					for (j=m_mesh/3; j<m_mesh-m_mesh/3; j++)
					{
						m_extrForces_u[1 + j*m_mesh] += 2.0f;
					}
				}

				// Rotation
				else if (force==5) 
				{
					float radius = ((float) m_mesh)/6.0f;
					for (float f=0.0f; f<(float)Math.PI; f+=0.01f)
					{
						float angle = 2.0f*f;
						float x = (float)Math.Cos(angle)*radius;
						float y = (float)Math.Sin(angle)*radius;
						//
						m_extrForces_u[(int)(m_mesh/2+x) + (int)(m_mesh/2+y)*m_mesh] -= (float)Math.Sin(angle)*0.2f;
						m_extrForces_v[(int)(m_mesh/2+x) + (int)(m_mesh/2+y)*m_mesh] += (float)Math.Cos(angle)*0.2f;	  
					}
				}
			}

			return;
		}



		/////////////////////////////////////////////////////////////////////////////////////////////
		// Original Simple Fluid Solver Code is by Jos Stam
		// 	  http://www.dgp.utoronto.ca/people/stam/reality/Research/pub.html
		//
		//    Using AForge.Math.FourierTransform for FFT
		//

		void SolveSFSW(int n, float[] wu,  float[] wv, float[] fu, float[] fv, int rsize, float visc, float dt)
		{
			for (int i=0; i<n*n; i++) {
				wu[i] += dt*fu[i];
				wv[i] += dt*fv[i];
				m_work_u[i] = wu[i]/rsize;
				m_work_v[i] = wv[i]/rsize;
			}

			for (int j=0; j<n; j++) {
				int jj = j*n;
				for (int i=0; i<n; i++) {
					float x = i - dt*m_work_u[i+jj]*n;
					float y = j - dt*m_work_v[i+jj]*n;
					//
					int  xi = (int)Math.Floor(x);
					int  yi = (int)Math.Floor(y);
					float s = (float)(x - xi);
					float t = (float)(y - yi);

					int  i0 = (n + (xi%n))%n;
					int  i1 = (i0 + 1)%n;
					int  j0 = (n + (yi%n))%n;
					int  j1 = (j0 + 1)%n;

					wu[i+jj] = (1.0f-s)*((1.0f-t)*m_work_u[i0+n*j0]+t*m_work_u[i0+n*j1]) 
									+ s*((1.0f-t)*m_work_u[i1+n*j0]+t*m_work_u[i1+n*j1]);
					wv[i+jj] = (1.0f-s)*((1.0f-t)*m_work_v[i0+n*j0]+t*m_work_v[i0+n*j1])
									+ s*((1.0f-t)*m_work_v[i1+n*j0]+t*m_work_v[i1+n*j1]);
				}
			}

			for (int j=0; j<n; j++) {
				int jj = j*n;
				for (int i=0; i<n; i++) {
					m_comp_u[i, j] = new AForge.Math.Complex(wu[i+jj], 0.0);
					m_comp_v[i, j] = new AForge.Math.Complex(wv[i+jj], 0.0);
				}
			}

			AForge.Math.FourierTransform.FFT2(m_comp_u, AForge.Math.FourierTransform.Direction.Forward);
			AForge.Math.FourierTransform.FFT2(m_comp_v, AForge.Math.FourierTransform.Direction.Forward);

			visc *= dt;
			for (int j=0; j<n; j++) {
				float y  = (float)(j<=n/2 ? j : j-n);
				float yy = y*y;
				//
				for (int i=0; i<n/2+1; i++) {
					float xx = i*i;
					float r_sq = xx + yy;
					if (r_sq!=0.0f) {
						float fac = (float)Math.Exp(-r_sq*visc);
						float xy  = i*y;

						AForge.Math.Complex ux = m_comp_u[i, j];
						AForge.Math.Complex vx = m_comp_v[i, j];
						m_comp_u[i, j] = fac*((1.0f-xx/r_sq)*ux        -xy/r_sq *vx);
						m_comp_v[i, j] = fac*(     -xy/r_sq *ux + (1.0f-yy/r_sq)*vx);
					}
				}
			}

			AForge.Math.FourierTransform.FFT2(m_comp_u, AForge.Math.FourierTransform.Direction.Backward);
			AForge.Math.FourierTransform.FFT2(m_comp_v, AForge.Math.FourierTransform.Direction.Backward);

			float nrml = 1.0f;//(n*n);
			for (int j=0; j<n; j++) {
				int jj = j*n;
				for (int i=0; i<n; i++) {
					wu[i+jj] = (float)(nrml*m_comp_u[i,j].Re)*rsize;
					wv[i+jj] = (float)(nrml*m_comp_v[i,j].Re)*rsize;
				}
			}

			return;
		}

	}
}
