namespace AIAnswerSheetAPI.Models
{
    public class AnswerSheet
    {
        public int Id { get; set; }
        public string StudentId { get; set; }
        public string Class { get; set; }
        public byte[] ImageData { get; set; }
        public string FileName { get; set; }
    }
}
