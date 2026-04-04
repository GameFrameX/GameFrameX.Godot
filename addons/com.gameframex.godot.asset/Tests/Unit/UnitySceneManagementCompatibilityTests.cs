using UnityEngine.SceneManagement;
using Xunit;

namespace GameFrameX.Asset.Tests.Unit;

public sealed class UnitySceneManagementCompatibilityTests
{
    [Fact]
    public void LoadScene_Additive_ShouldIncreaseSceneCount()
    {
        SceneManager.LoadScene("bootstrap", new LoadSceneParameters(LoadSceneMode.Single));

        SceneManager.LoadScene("sub_scene", new LoadSceneParameters(LoadSceneMode.Additive));

        Assert.Equal(2, SceneManager.sceneCount);
    }

    [Fact]
    public void UnloadSceneAsync_ShouldDecreaseSceneCount()
    {
        SceneManager.LoadScene("bootstrap", new LoadSceneParameters(LoadSceneMode.Single));
        var additiveScene = SceneManager.LoadScene("sub_scene", new LoadSceneParameters(LoadSceneMode.Additive));
        Assert.Equal(2, SceneManager.sceneCount);

        var unloadOperation = SceneManager.UnloadSceneAsync(additiveScene);

        Assert.NotNull(unloadOperation);
        Assert.True(unloadOperation.isDone);
        Assert.Equal(1, SceneManager.sceneCount);
    }

    [Fact]
    public void SetActiveScene_ShouldFailAfterSceneUnloaded()
    {
        SceneManager.LoadScene("bootstrap", new LoadSceneParameters(LoadSceneMode.Single));
        var additiveScene = SceneManager.LoadScene("sub_scene", new LoadSceneParameters(LoadSceneMode.Additive));
        SceneManager.UnloadSceneAsync(additiveScene);

        var activated = SceneManager.SetActiveScene(additiveScene);

        Assert.False(activated);
    }
}
