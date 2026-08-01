using Discord;
using Discord.Interactions;

namespace ValoShopTracker;

public class UrlInput : IModal
{
    public string Title => "Login";

    [InputLabel("URL")]
    [ModalTextInput("token_input", TextInputStyle.Short, placeholder: "Paste your url here")]
    public string Url { get; set; }
}