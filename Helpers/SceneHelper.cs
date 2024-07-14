using Godot;

public static class SceneHelper
{
    private const string SCENE_FOLDER = "res://Scenes/";
    private const string SCENE_SUFFIX = ".tscn";
	private const string CONTENT_FOLDER = "res://Objects/Content/";
	private const string CONTENT_FORMAT = ".png";
    public static void TransitionScene(Node node, string sceneName)
    {
		node.GetTree().ChangeSceneToPacked(
            (PackedScene)ResourceLoader.Load(SCENE_FOLDER + sceneName + SCENE_SUFFIX));
    }

    public static Texture2D LoadTexture2D(string item)
    {
        return ResourceLoader.Load<Texture2D>(CONTENT_FOLDER + item + CONTENT_FORMAT);
    }
}