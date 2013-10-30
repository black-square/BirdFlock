var pluginUrl="http://black-square.github.io/ext/BirdFlock/BoidResult.unity3d";
var config = {
  width: 960, 
  height: 600,
  params: { enableDebugging:"0", disableContextMenu: true }
};
var unityPlayer = null;

function LaunchUniy() {
  if(unityPlayer == null )
    unityPlayer = new UnityObject2(config);

  var $missingScreen = jQuery("#unityPlayer").find(".missing");
  var $brokenScreen = jQuery("#unityPlayer").find(".broken");
  var $playBtnScreen = jQuery("#unityPlayer").find(".playBtn");
  
  $missingScreen.hide();
  $brokenScreen.hide();
  $playBtnScreen.remove();
  
  unityPlayer.observeProgress(function (progress) {
    switch(progress.pluginStatus) {
      case "broken":
        $brokenScreen.find("a").click(function (e) {
          e.stopPropagation();
          e.preventDefault();
          unityPlayer.installPlugin();
          return false;
        });
        $brokenScreen.show();
      break;
      case "missing":
        $missingScreen.find("a").click(function (e) {
          e.stopPropagation();
          e.preventDefault();
          unityPlayer.installPlugin();
          return false;
        });
        $missingScreen.show();
      break;
      case "installed":
        $missingScreen.remove();
      break;
      case "first":
      break;
    }
  });
  unityPlayer.initPlugin(jQuery("#unityPlayer")[0], pluginUrl);
}