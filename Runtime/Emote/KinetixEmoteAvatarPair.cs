namespace Kinetix.Internal
{
    public struct KinetixEmoteAvatarPair
    {
        public KinetixAvatar Avatar;
        public KinetixEmote Emote;

        public KinetixEmoteAvatarPair(KinetixEmote _Emote, KinetixAvatar _Avatar)
        {
            Emote = _Emote;
            Avatar = _Avatar;
        }
    }
}
