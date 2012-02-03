// ============================================================
//  RRDSharp: Managed implementation of RRDTool for .NET/Mono
// ============================================================
//
// Project Info:  http://sourceforge.net/projects/rrdsharp/
// Project Lead:  Julio David Quintana (david@quintana.org)
//
// Distributed under terms of the LGPL:
//
// This library is free software; you can redistribute it and/or modify it under the terms
// of the GNU Lesser General Public License as published by the Free Software Foundation;
// either version 2.1 of the License, or (at your option) any later version.
//
// This library is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY;
// without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
// See the GNU Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License along with this
// library; if not, write to the Free Software Foundation, Inc., 59 Temple Place, Suite 330,
// Boston, MA 02111-1307, USA.

using System;

using RrdSharp.Core;

namespace RrdSharp.Graph
{
	/// <summary>
	/// 
	/// </summary>
	public class CubicSplineInterpolator : Plottable 
	{
		private double[] x;
		private double[] y;

		// second derivates come here
		private double[] y2;

		// internal spline variables
		private int n, klo, khi;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="timestamps"></param>
		/// <param name="values"></param>
		public CubicSplineInterpolator(long[] timestamps, double[] values)
		{
			this.x = new double[timestamps.Length];
			for(int i = 0; i < timestamps.Length; i++)
			{
				this.x[i] = timestamps[i];
			}
			this.y = values;
			Validate();
			Spline();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="dates"></param>
		/// <param name="values"></param>
		public CubicSplineInterpolator(DateTime[] dates, double[] values)
		{
			this.x = new double[dates.Length];
			for(int i = 0; i < dates.Length; i++)
			{
				this.x[i] = Util.GetTimestamp(dates[i]);
			}
			this.y = values;
			Validate();
			Spline();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		public CubicSplineInterpolator(double[] x, double[] y)
		{
			this.x = x;
			this.y = y;
			Validate();
			Spline();
		}

		private void Validate()
		{
			bool ok = true;
			if(x.Length != y.Length || x.Length < 3)
			{
				ok = false;
			}
			for(int i = 0; i < x.Length - 1 && ok; i++)
			{
				if(x[i] >= x[i + 1] || Double.IsNaN(y[i]))
				{
					ok = false;
				}
			}
			if(!ok)
			{
				throw new RrdException("Invalid plottable data supplied");
			}
		}

		private void Spline()
		{
			n = x.Length;
			y2 = new double[n];
			double[] u = new double[n - 1];
			y2[0] = y2[n - 1] = 0.0;
			u[0] = 0.0; // natural spline
			for (int i = 1; i <= n - 2; i++)
			{
				double sig = (x[i] - x[i - 1]) / (x[i + 1] - x[i - 1]);
				double p = sig * y2[i - 1] + 2.0;
				y2[i] = (sig - 1.0) / p;
				u[i] = (y[i + 1] - y[i]) / (x[i + 1] - x[i]) - (y[i] - y[i - 1]) / (x[i] - x[i - 1]);
				u[i] = (6.0 * u[i] / (x[i + 1] - x[i - 1]) - sig * u[i - 1]) / p;
			}
			for (int k = n - 2; k >= 0; k--)
			{
				y2[k] = y2[k] * y2[k + 1] + u[k];
			}
			// prepare everything for getValue()
			klo = 0;
			khi = n - 1;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="xval"></param>
		/// <returns></returns>
		public double GetValue(double xval)
		{
			if(xval < x[0] || xval > x[n - 1]) {
				return Double.NaN;
			}
			if(xval < x[klo] || xval > x[khi]) {
				// out of bounds
				klo = 0;
				khi = n - 1;
			}
			while (khi - klo > 1) {
				// find bounding interval using bisection method
				int k = (khi + klo) / 2;
				if (x[k] > xval) {
					khi = k;
				}
				else {
					klo = k;
				}
			}
			double h = x[khi] - x[klo];
			double a = (x[khi] - xval) / h;
			double b = (xval - x[klo]) / h;
			return a * y[klo] + b * y[khi] +
				((a * a * a - a) * y2[klo] + (b * b * b - b) * y2[khi]) * (h * h) / 6.0;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="timestamp"></param>
		/// <returns></returns>
		public override double GetValue(long timestamp)
		{
			return GetValue((double)timestamp);
		}

	}
}