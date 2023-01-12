public class StringSource
{
    public virtual string TheMove => "Move: {0}";
    public virtual string TheOptimal => "Optimal: {0}";
    public virtual string TheLevel => "Level {0}";

    public virtual string Play => "Play";
    public virtual string Quit => "Quit";
    public virtual string Restart => "Restart";
    public virtual string Back => "Back";

    public virtual string Settings => "Settings";
    public virtual string Language => "Language: English";
    public virtual string SoundOn => "Sound: On";
    public virtual string SoundOff => "Sound: Off";
    public virtual string MusicOn => "Music: On";
    public virtual string MusicOff => "Music: Off";
}

public class ChineseStringSource : StringSource
{
    public override string TheMove => "步数: {0}";
    public override string TheOptimal => "最优解: {0}";
    public override string TheLevel => "关卡 {0}";

    public override string Settings => "设置";
    public override string Play => "开始";
    public override string Quit => "退出";
    public override string Restart => "重玩";
    public override string Back => "返回";

    public override string Language => "语言: 简体中文";
    public override string SoundOn => "声音: 开";
    public override string SoundOff => "声音: 关";
    public override string MusicOn => "音乐: 开";
    public override string MusicOff => "音乐: 关";
}

public class EspanolStringSource : StringSource
{
    public override string TheMove => "Jugadas: {0}";
    public override string TheOptimal => "El óptimo: {0}";
    public override string TheLevel => "Nivel {0}";

    public override string Settings => "Configuración";
    public override string Play => "Jugar";
    public override string Quit => "Abandonar";
    public override string Restart => "Reiniciar";
    public override string Back => "Volver";

    public override string Language => "Idioma: Español";
    public override string SoundOn => "Sonido: Encendido";
    public override string SoundOff => "Sonido: Apagado";
    public override string MusicOn => "Música: Encendida";
    public override string MusicOff => "Música: Apagada";
}
