using R3;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
namespace LandoltRush
{
    public enum GameCommand { Start, Restart, Title, Pause, Mute, Quit }
    public sealed class GameInputInfo : MonoBehaviour
    {
        public Button StartButton,RestartButton,TitleButton,PauseButton,ResumeButton,MuteButton,QuitButton;
        readonly Subject<GameCommand> commands=new Subject<GameCommand>();
        public Observable<GameCommand> OnCommand=>commands;
        void Awake()
        {
            StartButton.onClick.AddListener(()=>Emit(GameCommand.Start));RestartButton.onClick.AddListener(()=>Emit(GameCommand.Restart));
            TitleButton.onClick.AddListener(()=>Emit(GameCommand.Title));PauseButton.onClick.AddListener(()=>Emit(GameCommand.Pause));
            ResumeButton.onClick.AddListener(()=>Emit(GameCommand.Pause));MuteButton.onClick.AddListener(()=>Emit(GameCommand.Mute));
            QuitButton.onClick.AddListener(()=>Emit(GameCommand.Quit));
        }
        void Update()
        {
            var k=Keyboard.current;if(k==null)return;
            if(k.enterKey.wasPressedThisFrame||k.spaceKey.wasPressedThisFrame)Emit(GameCommand.Start);
            if(k.rKey.wasPressedThisFrame)Emit(GameCommand.Restart);
            if(k.escapeKey.wasPressedThisFrame)Emit(GameCommand.Pause);
            if(k.mKey.wasPressedThisFrame)Emit(GameCommand.Mute);
        }
        public void Emit(GameCommand command)=>commands.OnNext(command);
        void OnDestroy()=>commands.Dispose();
    }
}
