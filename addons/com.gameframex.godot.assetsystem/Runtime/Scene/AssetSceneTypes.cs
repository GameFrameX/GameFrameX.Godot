namespace GameFrameX.AssetSystem
{
    public enum SceneLoadMode
    {
        Single = 0,
        Additive = 1
    }

    public enum ScenePhysicsMode
    {
        None = 0
    }

    public struct SceneLoadParameters
    {
        public SceneLoadParameters(SceneLoadMode sceneMode) : this(sceneMode, ScenePhysicsMode.None)
        {
        }

        public SceneLoadParameters(SceneLoadMode sceneMode, ScenePhysicsMode physicsMode)
        {
            SceneMode = sceneMode;
            PhysicsMode = physicsMode;
        }

        public SceneLoadMode SceneMode { get; set; }
        public ScenePhysicsMode PhysicsMode { get; set; }
    }

    public struct AssetSceneInfo
    {
        public string Name { get; set; }
        public bool IsLoaded { get; set; }
        internal bool IsValidFlag { get; set; }

        public bool IsValid()
        {
            return IsValidFlag;
        }
    }
}
