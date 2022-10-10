// File: PlayArcade.jslib

mergeInto(LibraryManager.library, {

    OnAppReady: function () {
      ReactUnityWebGL.OnAppReady();
    },

    RefreshUserInfo: function () {
      ReactUnityWebGL.RefreshUserInfo();
    }
  });