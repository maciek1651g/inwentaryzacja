using System;

namespace Inwentaryzacja.models
{
	public class Asset 
	{
		private ReportPosition ReportPostion;
		private ScanningPosition ScanningPosition;

		public int AssetId;
		public string Name;
		public int AssetType;

		public Asset(string name, int assetId, int assetType)
		{
			AssetId = assetId;
			Name = name;
			AssetType = assetType;
		}

		public bool Delete() 
		{
			throw new System.Exception("Not implemented");
		}
	}
}
