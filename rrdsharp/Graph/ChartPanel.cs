using System;

namespace RrdSharp.Graph
{
	public class ChartPanel
	{
		private BufferedImage chart;
	
		void setChart( BufferedImage chart ) 
		{
			this.chart = chart;
		}

		/**
		 * Overrides inhereted <code>paintComponent()</code> method from the base class.
		 * @param g Graphics object
		 */
		public void paintComponent( Graphics g )
		{
			if ( chart != null ) g.drawImage( chart, 0, 0, null );
		}
	}
}