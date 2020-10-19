using System;

namespace ConquestBuilder.Models
{
    public class FactionCarouselCard
    {
        public string Name { get; set; }
        public string ShortName { get; set; }
        public string ImageSource { get; set; }
        public string Text { get; set; }
        public Guid ID { get; }

        public FactionCarouselCard()
        {
            ID = Guid.NewGuid();
        }
    }
}
