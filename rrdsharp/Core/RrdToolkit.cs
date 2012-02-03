// Current with 1.3.1

using System;

namespace RrdSharp.Core
{
    
	public class RrdToolkit
	{
		private static RrdToolkit ourInstance;

		public static RrdToolkit getInstance()
		{
			lock(this)
			{
				if (ourInstance == null)
				{
					ourInstance = new RrdToolkit();
				}
				return ourInstance;
			}
		}


		private RrdToolkit()
		{
		}

		public void addDatasource(String sourcePath, String destPath, DsDef newDatasource)
		{
			if (Util.sameFilePath(sourcePath, destPath)) 
			{
				throw new RrdException("Source and destination paths are the same");
			}
			RrdDb rrdSource = new RrdDb(sourcePath);
			RrdDef rrdDef = rrdSource.getRrdDef();
			rrdDef.setPath(destPath);
			rrdDef.addDatasource(newDatasource);
			RrdDb rrdDest = new RrdDb(rrdDef);
			rrdSource.copyStateTo(rrdDest);
			rrdSource.close();
			rrdDest.close();
		}

		public void addDatasource(String sourcePath, DsDef newDatasource, boolean saveBackup)
		{
			String destPath = Util.getTmpFilename();
			addDatasource(sourcePath, destPath, newDatasource);
			copyFile(destPath, sourcePath, saveBackup);
		}

		public void removeDatasource(String sourcePath, String destPath, String dsName)
		{
			if (Util.sameFilePath(sourcePath, destPath)) 
			{
				throw new RrdException("Source and destination paths are the same");
			}
			RrdDb rrdSource = new RrdDb(sourcePath);
			RrdDef rrdDef = rrdSource.getRrdDef();
			rrdDef.setPath(destPath);
			rrdDef.removeDatasource(dsName);
			RrdDb rrdDest = new RrdDb(rrdDef);
			rrdSource.copyStateTo(rrdDest);
			rrdSource.close();
			rrdDest.close();
		}

		public void removeDatasource(String sourcePath, String dsName, boolean saveBackup)
		{
			String destPath = Util.getTmpFilename();
			removeDatasource(sourcePath, destPath, dsName);
			copyFile(destPath, sourcePath, saveBackup);
		}

		public void addArchive(String sourcePath, String destPath, ArcDef newArchive)
		{
			if (Util.sameFilePath(sourcePath, destPath)) 
			{
				throw new RrdException("Source and destination paths are the same");
			}
			RrdDb rrdSource = new RrdDb(sourcePath);
			RrdDef rrdDef = rrdSource.getRrdDef();
			rrdDef.setPath(destPath);
			rrdDef.addArchive(newArchive);
			RrdDb rrdDest = new RrdDb(rrdDef);
			rrdSource.copyStateTo(rrdDest);
			rrdSource.close();
			rrdDest.close();
		}

		public void addArchive(String sourcePath, ArcDef newArchive, boolean saveBackup)
		{
			String destPath = Util.getTmpFilename();
			addArchive(sourcePath, destPath, newArchive);
			copyFile(destPath, sourcePath, saveBackup);
		}

		public void removeArchive(String sourcePath, String destPath, String consolFun, int steps)
		{
			if (Util.sameFilePath(sourcePath, destPath)) 
			{
				throw new RrdException("Source and destination paths are the same");
			}
			RrdDb rrdSource = new RrdDb(sourcePath);
			RrdDef rrdDef = rrdSource.getRrdDef();
			rrdDef.setPath(destPath);
			rrdDef.removeArchive(consolFun, steps);
			RrdDb rrdDest = new RrdDb(rrdDef);
			rrdSource.copyStateTo(rrdDest);
			rrdSource.close();
			rrdDest.close();
		}

		public void removeArchive(String sourcePath, String consolFun, int steps, boolean saveBackup)
		{
			String destPath = Util.getTmpFilename();
			removeArchive(sourcePath, destPath, consolFun, steps);
			copyFile(destPath, sourcePath, saveBackup);
		}

		private static void copyFile(String sourcePath, String destPath, boolean saveBackup)
		{
			File source = new File(sourcePath);
			File dest = new File(destPath);
			if (saveBackup) 
			{
				String backupPath = destPath + ".bak";
				File backup = new File(backupPath);
				deleteFile(backup);
				if (!dest.renameTo(backup)) 
				{
					throw new IOException("Could not create backup file " + backupPath);
				}
			}
			deleteFile(dest);
			if (!source.renameTo(dest)) 
			{
				throw new IOException("Could not create file " + destPath + " from " + sourcePath);
			}
		}

		public void setDsHeartbeat(String sourcePath, String datasourceName, long newHeartbeat)
		{
			RrdDb rrd = new RrdDb(sourcePath);
			Datasource ds = rrd.getDatasource(datasourceName);
			ds.setHeartbeat(newHeartbeat);
			rrd.close();
		}

		public void setDsMinValue(String sourcePath, String datasourceName, double newMinValue, boolean filterArchivedValues)
		{
			RrdDb rrd = new RrdDb(sourcePath);
			Datasource ds = rrd.getDatasource(datasourceName);
			ds.setMinValue(newMinValue, filterArchivedValues);
			rrd.close();
		}

		public void setDsMaxValue(String sourcePath, String datasourceName, double newMaxValue, boolean filterArchivedValues)
		{
			RrdDb rrd = new RrdDb(sourcePath);
			Datasource ds = rrd.getDatasource(datasourceName);
			ds.setMaxValue(newMaxValue, filterArchivedValues);
			rrd.close();
		}

		public void setDsMinMaxValue(String sourcePath, String datasourceName,double newMinValue, double newMaxValue, boolean filterArchivedValues)
		{
			RrdDb rrd = new RrdDb(sourcePath);
			Datasource ds = rrd.getDatasource(datasourceName);
			ds.setMinMaxValue(newMinValue, newMaxValue, filterArchivedValues);
			rrd.close();
		}

		public void setArcXff(String sourcePath, String consolFun, int steps, double newXff)
		{
			RrdDb rrd = new RrdDb(sourcePath);
			Archive arc = rrd.getArchive(consolFun, steps);
			arc.setXff(newXff);
			rrd.close();
		}

		public void resizeArchive(String sourcePath, String destPath, String consolFun,
			int numSteps, int newRows)
		{
			if (Util.sameFilePath(sourcePath, destPath)) 
			{
				throw new RrdException("Source and destination paths are the same");
			}
			if (newRows < 2) 
			{
				throw new RrdException("New arcihve size must be at least 2");
			}
			RrdDb rrdSource = new RrdDb(sourcePath);
			RrdDef rrdDef = rrdSource.getRrdDef();
			ArcDef arcDef = rrdDef.findArchive(consolFun, numSteps);
			if (arcDef.getRows() != newRows) 
			{
				arcDef.setRows(newRows);
				rrdDef.setPath(destPath);
				RrdDb rrdDest = new RrdDb(rrdDef);
				rrdSource.copyStateTo(rrdDest);
				rrdDest.close();
			}
			rrdSource.close();
		}

		public void resizeArchive(String sourcePath, String consolFun, int numSteps, int newRows, boolean saveBackup)
		{
			String destPath = Util.getTmpFilename();
			resizeArchive(sourcePath, destPath, consolFun, numSteps, newRows);
			copyFile(destPath, sourcePath, saveBackup);
		}

		private static void deleteFile(File file)
		{
			if (file.exists() && !file.delete()) 
			{
				throw new IOException("Could not delete file: " + file.getCanonicalPath());
			}
		}
	}
}