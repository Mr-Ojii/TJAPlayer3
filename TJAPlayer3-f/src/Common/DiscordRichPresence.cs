using DiscordRPC;

namespace TJAPlayer3;

internal class DiscordRichPresence : IDisposable
{
    private static readonly string largeImageKey = "tjaplayer3-f";
    private static readonly string largeImageText = "Ver." + TJAPlayer3.VERSION + "(" + RuntimeInformation.RuntimeIdentifier + ")";

    private DateTime StartupTime;
    private DiscordRpcClient DiscordClient;

    public DiscordRichPresence(string applicationID)
    {
        this.DiscordClient = new DiscordRpcClient(applicationID);
        this.DiscordClient.Initialize();
        this.StartupTime = DateTime.UtcNow;
    }

    public void Update(string state) => this.Update("", state);
    public void Update(string details, string state) => this.Update(details, state, new Timestamps(this.StartupTime));
    public void Update(string details, string state, DateTime start, DateTime end, string smallImageKey = "", string smallImageText = "") => this.Update(details, state, new Timestamps(start, end), smallImageKey, smallImageText);
    public void Update(string details, string state, Timestamps timestamps, string smallImageKey = "", string smallImageText = "")
    {
        DiscordClient.SetPresence(new RichPresence()
        {
            Details = details,
            State = state,
            Timestamps = timestamps,
            Assets = new Assets()
            {
                LargeImageKey = largeImageKey,
                LargeImageText = largeImageText,
                SmallImageKey = smallImageKey,
                SmallImageText = smallImageText,
            }
        });
    }

    public void Dispose()
    {
        this.DiscordClient.Dispose();
    }
}
