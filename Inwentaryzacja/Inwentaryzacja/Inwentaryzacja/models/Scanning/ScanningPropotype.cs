using System;

namespace Inwentaryzacja.models
{
	public class ScanningPropotype
	{
		public int Id;
		public Room Room;
		public ScanningPosition[] Positions;

		public ScanningPropotype(int id, Room room, ScanningPosition[] positions)
		{
			Id = id;
			Room = room;
			Positions = positions;
		}
	}
}