namespace Kinetix.Internal
{
    public class EmoteRetargetingResponse<TResponseType> : OperationResponse
    {
        public TResponseType RetargetedClip;
        public long EstimatedClipSize;
    }

}
