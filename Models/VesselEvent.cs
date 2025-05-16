namespace FullPortManagementSystem.Models
{
    public class VesselEvent
    {
        public int Id { get; set; }
        public string vessel_type { get; set; }
        public int vessel_size { get; set; }
        public int eta_hour { get; set; }
        public int planned_departure_hour { get; set; }
        public string berth_id { get; set; }
        public string berth_type { get; set; }
        public int weather_score { get; set; }
    }
}
