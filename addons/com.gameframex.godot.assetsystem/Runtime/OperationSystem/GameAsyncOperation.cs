namespace GameFrameX.AssetSystem
{
    [AssetSystemPreserve]
    public abstract class GameAsyncOperation : AsyncOperationBase
    {
        [AssetSystemPreserve]
        public override void InternalOnStart()
        {
            OnStart();
        }

        [AssetSystemPreserve]
        public override void InternalOnUpdate()
        {
            OnUpdate();
        }

        [AssetSystemPreserve]
        internal override void InternalOnAbort()
        {
            OnAbort();
        }

        [AssetSystemPreserve]
        public override void InternalWaitForAsyncComplete()
        {
            OnWaitForAsyncComplete();
        }

        /// <summary>
        /// 异步操作开始
        /// </summary>
        [AssetSystemPreserve]
        protected abstract void OnStart();

        /// <summary>
        /// 异步操作更新
        /// </summary>
        [AssetSystemPreserve]
        protected abstract void OnUpdate();

        /// <summary>
        /// 异步操作终止
        /// </summary>
        [AssetSystemPreserve]
        protected abstract void OnAbort();

        /// <summary>
        /// 异步等待完成
        /// </summary>
        [AssetSystemPreserve]
        protected virtual void OnWaitForAsyncComplete()
        {
        }

        /// <summary>
        /// 异步操作系统是否繁忙
        /// </summary>
        [AssetSystemPreserve]
        protected bool IsBusy()
        {
            return OperationSystem.IsBusy;
        }
    }
}