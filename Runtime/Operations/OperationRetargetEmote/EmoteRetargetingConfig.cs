namespace Kinetix.Internal
{
    public class EmoteRetargetingConfig : OperationConfig
    {
        public readonly KinetixAvatar Avatar;
        public readonly KinetixEmote Emote;
        public readonly SequencerPriority Priority;
        public readonly string Path;
        public readonly SequencerCancel CancellationSequencer;
        public OperationConfig ResponseType;

        public EmoteRetargetingConfig(KinetixEmote _Emote, KinetixAvatar _Avatar, SequencerPriority _Priority, string _Path, SequencerCancel _CancellationSequencer)
        {
            Avatar = _Avatar;
            Emote = _Emote;
            Priority = _Priority;
            Path = _Path;
            CancellationSequencer = _CancellationSequencer;
        }
    }
}
