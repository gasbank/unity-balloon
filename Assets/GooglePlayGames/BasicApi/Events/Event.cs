namespace GooglePlayGames.BasicApi.Events
{
    internal class Event : IEvent
    {
        internal Event(string id, string name, string description, string imageUrl,
            ulong currentCount, EventVisibility visibility)
        {
            Id = id;
            Name = name;
            Description = description;
            ImageUrl = imageUrl;
            CurrentCount = currentCount;
            Visibility = visibility;
        }

        public string Id { get; }

        public string Name { get; }

        public string Description { get; }

        public string ImageUrl { get; }

        public ulong CurrentCount { get; }

        public EventVisibility Visibility { get; }
    }
}