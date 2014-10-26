﻿using System.Linq;
using System.Windows.Input;
using System.Windows.Threading;
using SimpleMusicPlayer.Base;
using SimpleMusicPlayer.Common;
using SimpleMusicPlayer.Interfaces;

namespace SimpleMusicPlayer.ViewModels
{
  public class PlayControlViewModel : ViewModelBase, IKeyHandler
  {
    private readonly MainViewModel mainViewModel;
    private readonly PlayListsViewModel playListsViewModel;
    private ICommand playOrPauseCommand;
    private ICommand stopCommand;
    private ICommand playPrevCommand;
    private ICommand playNextCommand;
    private ICommand shuffleCommand;
    private ICommand repeatCommand;
    private ICommand muteCommand;
    private ICommand showMediaLibraryCommand;

    public PlayControlViewModel(Dispatcher dispatcher, MainViewModel mainViewModel) {
      this.mainViewModel = mainViewModel;
      this.playListsViewModel = mainViewModel.PlayListsViewModel;
      this.PlayerEngine = mainViewModel.PlayerEngine;
      this.PlayerSettings = mainViewModel.PlayerSettings;

      this.PlayerEngine.PlayNextFileAction = () =>
                                             {
                                               var playerMustBeStoped = !this.CanPlayNext();
                                               if (!playerMustBeStoped)
                                               {
                                                 playerMustBeStoped = !this.PlayerSettings.PlayerEngine.ShuffleMode
                                                                      && !this.PlayerSettings.PlayerEngine.RepeatMode
                                                                      && this.playListsViewModel.IsFirstOrLastPlayListFile();
                                                 if (!playerMustBeStoped)
                                                 {
                                                   this.PlayNext();
                                                 }
                                               }
                                               if (playerMustBeStoped)
                                               {
                                                 this.Stop();
                                               }
                                             };
    }

    public PlayerEngine PlayerEngine { get; private set; }

    public PlayerSettings PlayerSettings { get; private set; }

    public ICommand PlayOrPauseCommand {
      get { return this.playOrPauseCommand ?? (this.playOrPauseCommand = new DelegateCommand(this.PlayOrPause, this.CanPlayOrPause)); }
    }

    private bool CanPlayOrPause() {
      return this.PlayerEngine.Initializied
             && this.playListsViewModel.FirstSimplePlaylistFiles != null
             && this.playListsViewModel.FirstSimplePlaylistFiles.OfType<IMediaFile>().Any();
    }

    private void PlayOrPause() {
      if (this.PlayerEngine.State == PlayerState.Pause || this.PlayerEngine.State == PlayerState.Play) {
        this.PlayerEngine.Pause();
      } else {
        var file = this.playListsViewModel.GetCurrentPlayListFile();
        if (file != null) {
          this.PlayerEngine.Play(file);
        }
      }
    }

    public ICommand StopCommand {
      get { return this.stopCommand ?? (this.stopCommand = new DelegateCommand(this.Stop, this.CanStop)); }
    }

    private bool CanStop() {
      return this.PlayerEngine.Initializied
             && this.playListsViewModel.FirstSimplePlaylistFiles != null
             && this.playListsViewModel.FirstSimplePlaylistFiles.OfType<IMediaFile>().Any();
    }

    private void Stop() {
      this.PlayerEngine.Stop();
      // at this we should re-set the current playlist item and the handsome selected file
      this.playListsViewModel.ResetCurrentItemAndSelection();
    }

    public ICommand PlayPrevCommand {
      get { return this.playPrevCommand ?? (this.playPrevCommand = new DelegateCommand(this.PlayPrev, this.CanPlayPrev)); }
    }

    private bool CanPlayPrev() {
      return this.PlayerEngine.Initializied
             && this.playListsViewModel.FirstSimplePlaylistFiles != null
             && this.playListsViewModel.FirstSimplePlaylistFiles.OfType<IMediaFile>().Any();
    }

    private void PlayPrev() {
      var file = this.playListsViewModel.GetPrevPlayListFile();
      if (file != null) {
        this.PlayerEngine.Play(file);
      }
    }

    public ICommand PlayNextCommand {
      get { return this.playNextCommand ?? (this.playNextCommand = new DelegateCommand(this.PlayNext, this.CanPlayNext)); }
    }

    private bool CanPlayNext() {
      return this.PlayerEngine.Initializied
             && this.playListsViewModel.FirstSimplePlaylistFiles != null
             && this.playListsViewModel.FirstSimplePlaylistFiles.OfType<IMediaFile>().Any();
    }

    private void PlayNext() {
      var file = this.playListsViewModel.GetNextPlayListFile();
      if (file != null) {
        this.PlayerEngine.Play(file);
      }
    }

    public ICommand ShuffleCommand {
      get { return this.shuffleCommand ?? (this.shuffleCommand = new DelegateCommand(this.SetShuffelMode, this.CanSetShuffelMode)); }
    }

    private bool CanSetShuffelMode() {
      return this.PlayerEngine.Initializied;
    }

    private void SetShuffelMode() {
      this.PlayerSettings.PlayerEngine.ShuffleMode = !this.PlayerSettings.PlayerEngine.ShuffleMode;
    }

    public ICommand RepeatCommand {
      get { return this.repeatCommand ?? (this.repeatCommand = new DelegateCommand(this.SetRepeatMode, this.CanSetRepeatMode)); }
    }

    public bool CanSetRepeatMode() {
      return this.PlayerEngine.Initializied;
    }

    public void SetRepeatMode() {
      this.PlayerSettings.PlayerEngine.RepeatMode = !this.PlayerSettings.PlayerEngine.RepeatMode;
    }

    public ICommand MuteCommand {
      get { return this.muteCommand ?? (this.muteCommand = new DelegateCommand(this.SetMute, this.CanSetMute)); }
    }

    public bool CanSetMute() {
      return this.PlayerEngine.Initializied;
    }

    public void SetMute() {
      //this.PlayerSettings.PlayerEngine.RepeatMode = !this.PlayerSettings.PlayerEngine.RepeatMode;
      this.PlayerEngine.IsMute = !this.PlayerEngine.IsMute;
    }

    public ICommand ShowMediaLibraryCommand {
      get { return this.showMediaLibraryCommand ?? (this.showMediaLibraryCommand = new DelegateCommand(this.mainViewModel.ShowMediaLibrary, this.CanShowMediaLibrary)); }
    }

    public bool CanShowMediaLibrary() {
      return true;
    }

    public bool HandleKeyDown(Key key) {
      var handled = false;
      switch (key) {
        case Key.R:
          handled = this.RepeatCommand.CanExecute(null);
          if (handled) {
            this.RepeatCommand.Execute(null);
          }
          break;
        case Key.S:
          handled = this.ShuffleCommand.CanExecute(null);
          if (handled) {
            this.ShuffleCommand.Execute(null);
          }
          break;
        case Key.J:
          handled = this.PlayNextCommand.CanExecute(null);
          if (handled) {
            this.PlayNextCommand.Execute(null);
          }
          break;
        case Key.K:
          handled = this.PlayPrevCommand.CanExecute(null);
          if (handled) {
            this.PlayPrevCommand.Execute(null);
          }
          break;
        case Key.M:
          handled = this.MuteCommand.CanExecute(null);
          if (handled) {
            this.MuteCommand.Execute(null);
          }
          break;
        case Key.Space:
          handled = this.PlayOrPauseCommand.CanExecute(null);
          if (handled) {
            this.PlayOrPauseCommand.Execute(null);
          }
          break;
        case Key.L:
          handled = this.ShowMediaLibraryCommand.CanExecute(null);
          if (handled) {
            this.ShowMediaLibraryCommand.Execute(null);
          }
          break;
      }
      return handled;
    }
  }
}