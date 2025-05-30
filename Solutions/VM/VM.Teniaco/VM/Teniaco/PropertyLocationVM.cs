﻿using FrameWork;

namespace VM.Teniaco
{
    public class PropertyLocationVM : BaseEntity
    {
        public long PropertyId { get; set; }
        public int StateId { get; set; }
        public int CityId { get; set; }
        public int? ZoneId { get; set; }
        public int? DistrictId { get; set; }
        public string? Address { get; set; }
        public double? LocationLat { get; set; }
        public string? TownName { get; set; }
        public double? LocationLon { get; set; }
    }
}
