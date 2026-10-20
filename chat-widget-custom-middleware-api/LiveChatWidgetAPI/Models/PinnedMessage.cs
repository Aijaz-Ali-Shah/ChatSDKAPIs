namespace LiveChatWidgetAPI.Models
{
    public class PinnedMessageResponse
    {
        public List<PinnedMessage> Value { get; set; } = new();
    }

    public class PinnedMessage
    {
        public string tvs_message { get; set; }
        public int tvs_customertype { get; set; }
        public string tvs_pinnedmessagename { get; set; }
        public int statuscode { get; set; }
        public string _tvs_language_value { get; set; }
        public int tvs_messagetype { get; set; }
        public DateTime? tvs_startdate { get; set; }
        public DateTime? tvs_enddate { get; set; }

        public List<LiveWorkstream> tvs_PinnedMessage_msdyn_liveworkstream_msdyn_liveworkstream { get; set; }
            = new();
    }

    public class LiveWorkstream
    {
        public string msdyn_liveworkstreamid { get; set; }
        public string msdyn_name { get; set; }
    }
}
