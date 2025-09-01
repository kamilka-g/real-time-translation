using Microsoft.AspNetCore.SignalR;

namespace EchoTravel.Api.Hubs
{
    /// <summary>
    /// Provides helper methods for deriving SignalR group names for trains and their cars.
    /// </summary>
    public static class TrainGroups
    {
        /// <summary>
        /// Returns the SignalR group name for the specified train and optional car.
        /// </summary>
        /// <param name="trainId">The train identifier.</param>
        /// <param name="car">Optional car identifier; if null, a group for the whole train is returned.</param>
        /// <returns>Group name string.</returns>
        public static string GetTrainGroupName(string trainId, string? car = null)
        {
            return car is null ? $"train.{trainId}" : $"train.{trainId}.car.{car}";
        }
    }

    /// <summary>
    /// SignalR hub used to manage subscriptions and publish train announcements to clients.
    /// </summary>
    public class AnnouncementsHub : Hub
    {
        /// <summary>
        /// Called when a client connects and wants to join a specific train (and optionally car) group.
        /// </summary>
        /// <param name="trainId">The train identifier.</param>
        /// <param name="car">Optional car identifier.</param>
        public async Task JoinTrain(string trainId, string? car = null)
        {
            var groupName = TrainGroups.GetTrainGroupName(trainId, car);
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            await Clients.Caller.SendAsync("connected", new { trainId, car, group = groupName });
        }

        /// <summary>
        /// Broadcasts an announcement message to the appropriate train/car group. Typically invoked from the server.
        /// </summary>
        /// <param name="trainId">The train identifier.</param>
        /// <param name="text">The announcement text.</param>
        /// <param name="car">Optional car identifier.</param>
        public async Task BroadcastAnnouncement(string trainId, string text, string? car = null)
        {
            var groupName = TrainGroups.GetTrainGroupName(trainId, car);
            var announcement = new
            {
                text = text,
                trainId = trainId,
                car = car,
                timestamp = DateTimeOffset.UtcNow
            };
            await Clients.Group(groupName).SendAsync("announcement", announcement);
        }
    }
}
