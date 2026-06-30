using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace MarbleHero;

public interface IText
{
    public string text { get; set; }
    public Color color { get; set; }
    public float fontSize { get; set; }
    public float outlineSize { get; set; }
    public Color outlineColor { get; set; }
}

public class FText : Transformable
{
    public int useTimes = -1;

    Image _icon;
    TextMeshProUGUI _tmp;
    TextTMP _text;
    Transform _content;
    CanvasGroup _canvas;
    Vector2 _rectPos;
    Vector3 _screenPos;

    public override void resetProperty()
    {
        base.resetProperty();
        useTimes = -1;
        _icon = null;
        _tmp = null;
        UN_CLASS(ref _text);
        _content = null;
        _canvas = null;
        _rectPos = default;
        _screenPos = default;
        
        
        _data = default;
        _pct = _acuPct = _totalPct = 0;
        _tempAcu = 0;
        _timeElapsed = 0;
        _baseAlpha = 0;
        _state = State.None;
    }

    public override void setObject(GameObject obj)
    {
        base.setObject(obj);
        obj.find(out _tmp, "Content/Text");
        obj.find(out _content, "Content");
        obj.find(out _icon, "Content/Icon");
        obj.TryGetComponent(out _canvas);
        CLASS(out _text).with(_tmp, true);
    }

    public void Set(Data data)
    {
        //get a random float direction
        var setting = data.setting;

        if (_icon)
        {
            if (setting.Icons.tryGet(data.type, out var sprite))
            {
                _icon.sprite = sprite;
                _icon.gameObject.SetActive(true);
            }
            else
            {
                _icon.sprite = null;
                _icon.gameObject.SetActive(false);
            }
        }

        _text.text = data.text;

        setContentScale(setting.ContentScale, data.extraContentSize);

        if (data.fontColor != default)
            _text.color = data.fontColor;
        else
            _text.color = setting.FontColors[data.type];

        if (data.outlineColor != default)
            _text.outlineColor = data.outlineColor;

        if (data.outlineSize != 0)
            _text.outlineSize = data.outlineSize;

        var result = CheckForReuse(data);

        setActive(true);

        //if this is a Reuse and is marked as not rewind on reuses
        if (result == 2 && data.hasFlag(Data.Flags.DontRewind))
            return;

        //set the start position
        var p = worldToScreen(data.getPosition(), false);
        setWorldPosition(p);
        data.initialPos = getWorldPosition();
        _screenPos = getWorldPosition();

        if (data.hasFlag(Data.Flags.InvertHorizontalDirectionRandomly))
            data.invertHorizontalDirection = Random.value > 0.5f;
        else
            data.invertHorizontalDirection = data.direction.x > 0;

        setting.ModifyFloatDirection(ref data);

        Setup(data);
    }

    /// <summary>
    /// Check if we can reuse this text instance
    /// </summary>
    /// <param name="data"></param>
    int CheckForReuse(Data data)
    {
        if (data.target == null)
            return 0;

        if (data.reuseTimes <= 0)
            return 0;

        switch (useTimes)
        {
            case -1:
                //if is the first time using this instance
                textManager.addToReused(this, data);
                useTimes = data.reuseTimes;
                break;
            case > 0:
                //if this is a re-use
                useTimes--;
                return 2;
            case 0:
                //if this is the last allow re-use time
                textManager.removeFromReused(data);
                return 2;
            case -2:
                //we can reuse this instance until the sequence finish
                return 2;
        }

        return 1;
    }


    enum State
    {
        None,
        Start,
        Static,
        Floating,
        Finishing,
        Finished,
    }

    Data _data;
    float _pct, _acuPct, _totalPct;
    float _tempAcu;
    float _timeElapsed;
    float _baseAlpha;
    State _state;

    void Setup(Data data)
    {
        _data = data;
        var conf = _data.setting;
        var startDuration = conf.StartSequenceDuration;
        var staticDuration = conf.StaticDuration;
        var floatingDuration = conf.FloatingDuration;
        var finishDuration = conf.FinishSequenceDuration;
        var totalDuration = startDuration + floatingDuration + finishDuration;

        _pct = 0F;
        _totalPct = 0F;
        _acuPct = startDuration / totalDuration;
        _tempAcu = 0F;
        _rectPos = Vector2.zero;
        _timeElapsed = 0F;
        _baseAlpha = 0F;
        _state = State.Start;
    }

    public override void update(float elapsedTime)
    {
        base.update(elapsedTime);
        if (_state == State.None)
            return;

        if (!_data.valid)
            return;

        var data = _data;
        var conf = data.setting;
        var startDuration = conf.StartSequenceDuration;
        var staticDuration = conf.StaticDuration;
        var floatingDuration = conf.FloatingDuration;
        var finishDuration = conf.FinishSequenceDuration;
        var totalDuration = startDuration + floatingDuration + finishDuration;

        if (_state == State.Start)
        {
            if (startDuration > 0)
            {
                if (_pct < 1)
                {
                    _pct += elapsedTime / startDuration;
                    setAlpha(conf.FadeInCurve.Evaluate(_pct));
                    setScale(conf.StartScaleCurve.Evaluate(_pct));

                    if (conf.CalculateTotalPct)
                        _totalPct = _acuPct * _pct;

                    if (staticDuration <= 0)
                        setPosition(data, _totalPct);
                    else
                    {
                        _screenPos = worldToScreen(data.getPosition(), false);
                        setWorldPosition(_screenPos);
                    }

                    return;
                }
            }

            toStaticState();
        }

        if (_state == State.Static)
        {
            if (staticDuration > 0)
            {
                if (data.hasFlag(Data.Flags.FollowScreen))
                {
                    _timeElapsed += elapsedTime;
                    if (_timeElapsed < staticDuration)
                        return;

                    toFloatingState();
                }
                else
                {
                    if (_pct < 1)
                    {
                        _pct += elapsedTime / staticDuration;
                        _screenPos = worldToScreen(data.getPosition(), false);
                        setWorldPosition(_screenPos);
                        return;
                    }

                    toFloatingState();
                }
            }
            else
            {
                toFloatingState();
            }
        }

        if (_state == State.Floating)
        {
            if (floatingDuration > 0)
            {
                if (_pct < 1)
                {
                    _pct += elapsedTime / floatingDuration;

                    setAlpha(conf.FadeOverLifeTime.Evaluate(_pct));

                    if (conf.CalculateTotalPct)
                        _totalPct = _acuPct + _tempAcu * _pct;

                    setPosition(data, _totalPct);
                    return;
                }
            }

            toFinishingState();
        }

        if (_state == State.Finishing)
        {
            if (finishDuration > 0)
            {
                if (_pct < 1)
                {
                    _pct += elapsedTime / finishDuration;
                    setAlpha(conf.FadeOutCurve.Evaluate(_pct) * _baseAlpha);
                    setScale(conf.FinishScaleCurve.Evaluate(_pct));

                    if (conf.CalculateTotalPct)
                        _totalPct = _acuPct + _tempAcu * _pct;

                    setPosition(data, _totalPct);
                    return;
                }
            }

            toFinishedState();
        }

        if (_state == State.Finished)
        {
            data.onFinish?.Invoke();
            useTimes = -1;
            setActive(false);

            textManager.release(this);
            toNoneState();
        }

        return;

        void toStaticState()
        {
            _pct = 0F;
            _state = State.Static;
        }

        void toFloatingState()
        {
            _pct = 0F;
            _tempAcu = floatingDuration / totalDuration;
            if (staticDuration > 0)
            {
                _tempAcu += _acuPct;
                _acuPct = 0F;
            }

            _state = State.Floating;
        }

        void toFinishingState()
        {
            _acuPct += _tempAcu;
            _pct = 0F;
            _baseAlpha = _canvas.alpha;
            _tempAcu = finishDuration / totalDuration;
            if (useTimes == -2)
                textManager.removeFromReused(data);

            _state = State.Finishing;
        }

        void toFinishedState()
        {
            _state = State.Finished;
        }

        void toNoneState()
        {
            _state = State.None;
            _data = default;
        }
    }

    void setPosition(Data data, float pct)
    {
        if (!data.hasFlag(Data.Flags.FollowScreen))
        {
            _screenPos = worldToScreen(data.getPosition(), false);
        }

        var setting = data.setting;
        setting.ModifyPosition(ref _rectPos, in data, pct);
        if (data.invertHorizontalDirection)
            _rectPos.x = -_rectPos.x;

        var pos = _screenPos + (Vector3)_rectPos;
        setWorldPosition(pos);
    }

    public override void setAlpha(float alpha)
    {
        _canvas.alpha = alpha;
    }

    void setContentScale(float scale, float deltaScale)
    {
        if (_content)
        {
            _content.localScale = (scale + deltaScale) * Vector3.one;
        }
    }

    public void Clear()
    {
        useTimes = -1;
    }

    public struct Data
    {
        [Flags]
        public enum Flags
        {
            None = 0,
            DontRewind = 1,
            FollowScreen = 2,
            InvertHorizontalDirectionRandomly = 4,
        }

        public bool valid;
        public int type;
        public float value;
        public string text;
        public Transform target;
        public FTextSetting setting;
        public Color fontColor;
        public float extraContentSize;
        public int reuseTimes;
        public float outlineSize;
        public Color outlineColor;
        public Action onFinish;

        public Vector3 initialPos;
        public Vector2 floatDirection;
        public bool invertHorizontalDirection;
        public Vector3 direction;

        Vector3 position;
        Vector3 offset;
        Flags flag;


        public Data(string content)
        {
            valid = true;
            type = 0;
            value = 0;
            target = null;
            direction = Vector3.zero;
            position = Vector3.zero;
            text = content;

            offset = Vector3.zero;
            extraContentSize = 0F;
            reuseTimes = 0;
            setting = null;
            fontColor = default;
            outlineColor = default;
            outlineSize = 0;

            initialPos = default;
            floatDirection = default;
            invertHorizontalDirection = false;

            onFinish = null;
            flag = Flags.None;
        }

        public void show()
        {
            if (string.IsNullOrEmpty(text))
                return;

            if (setting)
            {
                textManager.show(this);
            }
        }

        public Data setType(int _type)
        {
            type = _type;
            return this;
        }

        public Data setValue(float _value)
        {
            value = _value;
            return this;
        }

        public Data setText(string _text)
        {
            text = _text;
            return this;
        }

        public Vector3 getPosition()
        {
            if (target)
                return target.position + offset;

            return position + offset;
        }

        public Data setTarget(Transform _target)
        {
            target = _target;

            if (target)
                position = target.position;

            return this;
        }

        public Data setDirection(Vector3 _direction)
        {
            direction = _direction;
            return this;
        }

        public Data setPosition(Vector3 _position)
        {
            position = _position;
            return this;
        }

        public Data setColor(Color color)
        {
            fontColor = color;
            return this;
        }

        public Data setOffset(Vector3 _offset)
        {
            offset = _offset;
            return this;
        }

        public Data setExtraContentSize(float size)
        {
            extraContentSize = size;
            return this;
        }

        /// <summary>
        /// Set reuses times
        /// Means how many times a text will use the same text instance instead of create a new one
        /// when the floating text is created for the same target within a short period of time.
        /// </summary>
        public Data setReuseTimes(int reuses)
        {
            reuseTimes = reuses;
            return this;
        }

        public Data setSetting(string settingName)
        {
            setting = textManager.getSetting(settingName);
            return this;
        }

        public Data setSetting(FTextSetting _setting)
        {
            setting = _setting;
            return this;
        }

        public Data setOutlineSize(float size)
        {
            outlineSize = size;
            return this;
        }

        public Data setOutlineColor(Color color)
        {
            outlineColor = color;
            return this;
        }

        /// <summary>
        /// Make reuse this instance while the text is showing
        /// </summary>
        public Data ReuseWhileAlive()
        {
            reuseTimes = -2;
            return this;
        }

        /// <summary>
        /// Don't replay the sequence/floating when re-used this instance
        /// </summary>
        public Data DontRewindOnReuse()
        {
            flag |= Flags.DontRewind;
            return this;
        }

        /// <summary>
        /// Make this text stick at the original world position and not to the screen position
        /// </summary>
        public Data setFollowScreen()
        {
            flag |= Flags.FollowScreen;
            return this;
        }

        public Data setInvertHorizontalDirectionRandomly()
        {
            flag |= Flags.InvertHorizontalDirectionRandomly;
            return this;
        }

        public Data OnFinish(Action callback)
        {
            onFinish += callback;
            return this;
        }

        public bool hasFlag(Flags flags)
        {
            return (flag & flags) != 0;
        }
    }
}