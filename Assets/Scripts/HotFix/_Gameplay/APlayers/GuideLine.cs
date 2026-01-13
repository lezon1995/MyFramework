using Drawing;
using UnityEngine;

namespace MarbleHero;

public class GuideLine : MovableObject
{
    static int MainTex = Shader.PropertyToID("_MainTex");

    COMMovableObjectDrag movableDrag;
    APlayer player;

    Material lineMat;
    Transform linesGroup;
    LineRenderer[] lines;
    Transform[] dots;
    Transform indicatorBall;
    DragStartCallback dragStartCallback;
    DragEndCallback dragEndCallback;
    DragCallback dragCallback;

    LayerMask _mask0, _mask1, _mask2, _mask3, _hit2Mask;

    float distance = 10f;
    float minShootDirectionLimitAngle = defaultMinShootDirectionLimitAngle;
    float maxShootDirectionLimitAngle = defaultMaxShootDirectionLimitAngle;
    const float defaultMinShootDirectionLimitAngle = -83F;
    const float defaultMaxShootDirectionLimitAngle = 83F;
    bool isLine;
    bool isOff;
    Vector3 originalShootPosition, shootPosition;
    Vector3 rawShootDirection, shootDirection;
    Vector3 dragPos;

    public override void onCtor()
    {
        base.onCtor();
        dragStartCallback = DragStartCallback;
        dragEndCallback = DragEndCallback;
        dragCallback = DragCallback;
    }

    public override void init()
    {
        base.init();

        enableMoveInfo();
        setHandleInput(true);

        var left = BORDER_LEFT_LAYER_MASK;
        var top = BORDER_TOP_LAYER_MASK;
        var right = BORDER_RIGHT_LAYER_MASK;
        var brick = BRICK_LAYER_MASK;
        // var brick = 0;
        _mask0 = left | top | right | brick;
        _mask1 = top | left | brick;
        _mask2 = top | right | brick;
        _mask3 = left | right | brick;

        mGlobalTouchSystem.registeCollider(this);
    }

    public void enableDragCallback()
    {
        movableDrag.setDragStartCallback(dragStartCallback);
        movableDrag.setDraggingCallback(dragCallback);
        movableDrag.setDragEndCallback(dragEndCallback);
    }

    public void disableDragCallback()
    {
        movableDrag.setDragStartCallback(null);
        movableDrag.setDraggingCallback(null);
        movableDrag.setDragEndCallback(null);
    }

    void DragEndCallback(ComponentOwner owner, Vector3 pos, bool cancel)
    {
        if (!room.inPlayerTurn)
            return;

        //if (CtrGame.instance.isGameOver || !CtrGame.instance.isGameStart) return;
        player.shootBalls(shootPosition, shootDirection);
    }

    void DragCallback(ComponentOwner owner, Vector3 pos)
    {
        if (!room.inPlayerTurn)
            return;

        dragPos = screenToWorld(pos, false);
        updateShootDirection(dragPos);
    }

    void updateShootDirection(Vector3 targetPos)
    {
        var diff = targetPos - originalShootPosition;
        diff.Normalize();
        var rotZ = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;
        var rotation = Quaternion.Euler(0f, 0f, Mathf.Clamp((rotZ - 90), minShootDirectionLimitAngle, maxShootDirectionLimitAngle));
        shootDirection = rotation * Vector3.up;

        var rotationRaw = Quaternion.Euler(0f, 0f, (rotZ - 90));
        rawShootDirection = rotationRaw * Vector3.up;
        Draw.ingame.xy.Line(originalShootPosition, originalShootPosition + shootDirection * 5, Color.green);
        Draw.ingame.xy.Line(shootPosition, shootPosition + shootDirection * 5, Color.red);
    }

    void DragStartCallback(ComponentOwner owner, TouchPoint point, ref bool drag)
    {
        if (!room.inPlayerTurn)
            return;

        //if (CtrGame.instance.isGameOver || !CtrGame.instance.isGameStart) return;
        //if (!CtrGame.instance.PlayerReady) return;
    }

    protected override void initComponents()
    {
        base.initComponents();

        addInitComponent(out movableDrag, true);
    }

    public override void destroy()
    {
        base.destroy();
        mGlobalTouchSystem.unregisteCollider(this);
    }

    public override void setObject(GameObject obj)
    {
        base.setObject(obj);
        lines = new LineRenderer[4];
        findComponent(obj, "IndicatorBall", out indicatorBall);
        findComponent(obj, "Group", out linesGroup);
        findComponent(obj, "Line0", out lines[0]);
        findComponent(obj, "Line1", out lines[1]);
        findComponent(obj, "Line2", out lines[2]);
        findComponent(obj, "Line3", out lines[3]);

        dots = new Transform[2];
        findComponent(obj, "Dot0", out dots[0]);
        findComponent(obj, "Dot1", out dots[1]);

        // lines[2].enabled = false;
        // lines[3].enabled = false;

        lines[0].positionCount = 4;
        lines[1].positionCount = 2;
        lines[2].positionCount = 2;
        lines[3].positionCount = 2;

        lineMat = lines[0].sharedMaterial;
    }

    public override void update(float elapsedTime)
    {
        base.update(elapsedTime);

        if (isOff)
            return;

        //Material Anim
        float offset = Time.time * -1f;
        lineMat.mainTextureScale = new(1, 1);
        lineMat.SetTextureOffset(MainTex, new(offset, 0f));

        refreshGuideLine();
    }

    void refreshGuideLine()
    {
        Vector2 origin = shootPosition;
        updateShootDirection(dragPos);
        Vector2 up = shootDirection;
        Vector2 dir = origin - (up * -distance);

        var radius = 0.14F;
        var hit = Physics2D.CircleCast(origin, radius, up, distance, _mask0);
        if (hit)
        {
            hit.point = hit.point + hit.normal * radius;

            Vector2 reflectFirstPos = Vector2.Reflect(hit.point - origin, hit.normal);
            Vector2 firstPosition = hit.point;

            float rayDistance = (origin - hit.point).magnitude;
            float resultRay = distance - rayDistance;

            firstPosition += reflectFirstPos.normalized * resultRay;

            var hitObj = hit.transform.gameObject;
            if (isBorder(hitObj))
            {
                if (hit.point.x > 0)
                {
                    //第一次射线检测到屏幕右侧
                    _hit2Mask = _mask1;
                }
                else
                {
                    //第一次射线检测到屏幕左侧
                    _hit2Mask = _mask2;
                }
            }
            else if (isBrick(hitObj))
            {
                _hit2Mask = _mask0;
            }
            else
            {
                _hit2Mask = _mask0;
            }

            var hit2 = Physics2D.CircleCast(hit.point, radius, reflectFirstPos, resultRay, _hit2Mask);
            if (hit2)
            {
                hit2.point = hit2.point + hit2.normal * radius;
                float rayDistance2 = (hit.point - hit2.point).magnitude;
                Vector2 reflectSecondPos = Vector2.Reflect(hit2.point - hit.point, hit2.normal);
                Vector2 secondPosition = hit2.point;
                float distanceRay2 = resultRay - rayDistance2;
                secondPosition += reflectSecondPos.normalized * distanceRay2;

                lineMode2(hit.point, hit2.point, secondPosition);
                // lineMode1(hit.point, hit2.point);
            }
            else
            {
                lineMode1(hit.point, firstPosition);
            }
        }
        else
        {
            lineMode0(dir);
        }
    }

    static bool isBorder(GameObject o)
    {
        return (ALL_BORDER_LAYER_MASK & (1 << o.layer)) != 0;
    }

    static bool isBrick(GameObject o)
    {
        return (BRICK_LAYER_MASK & (1 << o.layer)) != 0;
    }

    void lineMode0(Vector3 dir)
    {
        if (_modeCount != 0)
        {
            _modeCount = 0;

            if (!isLine)
                lineColor(0);

            lines[0].positionCount = 4;
            lines[1].positionCount = 2;
            lines[2].positionCount = 0;
            lines[3].positionCount = 0;
        }

        if (dots[0].gameObject.activeSelf)
            dots[0].gameObject.SetActive(false);

        if (dots[1].gameObject.activeSelf)
            dots[1].gameObject.SetActive(false);

        var position = shootPosition;
        lines[0].SetPosition(0, position);
        lines[0].SetPosition(1, dir);
        lines[0].SetPosition(2, dir);
        lines[0].SetPosition(3, dir);

        lines[1].SetPosition(0, position);
        lines[1].SetPosition(1, dir);
    }

    //hit + forward
    void lineMode1(Vector3 hit, Vector3 dir)
    {
        if (_modeCount != 1)
        {
            _modeCount = 1;
            if (!isLine)
                lineColor(1);

            lines[0].positionCount = 3;
            lines[1].positionCount = 2;
            lines[2].positionCount = 2;
            lines[3].positionCount = 0;
        }

        lines[0].SetPosition(0, shootPosition);
        lines[0].SetPosition(1, hit);
        lines[0].SetPosition(2, dir);

        if (!dots[0].gameObject.activeSelf)
            dots[0].gameObject.SetActive(true);

        if (dots[1].gameObject.activeSelf)
            dots[1].gameObject.SetActive(false);

        dots[0].transform.position = hit;

        lines[1].SetPosition(0, shootPosition);
        lines[1].SetPosition(1, hit);
        lines[2].SetPosition(0, hit);
        lines[2].SetPosition(1, dir);
    }

    //hit + hit + forward
    int _modeCount = -1;

    void lineMode2(Vector3 hit1, Vector3 hit2, Vector3 dir)
    {
        if (_modeCount != 2)
        {
            _modeCount = 2;
            lines[0].positionCount = 4;
            lines[1].positionCount = 2;
            lines[2].positionCount = 2;
            lines[3].positionCount = 2;
        }

        if (!isLine)
            lineColor(2);

        lines[0].SetPosition(0, shootPosition);
        lines[0].SetPosition(1, hit1);
        lines[0].SetPosition(2, hit2);
        lines[0].SetPosition(3, dir);

        if (!dots[0].gameObject.activeSelf)
            dots[0].gameObject.SetActive(true);

        if (!dots[1].gameObject.activeSelf)
            dots[1].gameObject.SetActive(true);

        dots[0].transform.position = hit1;
        dots[1].transform.position = hit2;

        lines[1].SetPosition(0, shootPosition);
        lines[1].SetPosition(1, hit1);
        lines[2].SetPosition(0, hit1);
        lines[2].SetPosition(1, hit2);
        lines[3].SetPosition(0, hit2);
        lines[3].SetPosition(1, dir);
    }


    #region - LinerAlpha

    int _lineType = -1;

    void lineColor(int type = 0)
    {
        if (_lineType == type)
            return;

        _lineType = type;

        switch (type)
        {
            case 0:
            {
                var gradient = lines[1].colorGradient;
                var alphaKeys = gradient.alphaKeys;

                alphaKeys[0].alpha = 1f;
                alphaKeys[0].time = 0f;
                alphaKeys[1].alpha = 0f;
                alphaKeys[1].time = 1f;

                gradient.alphaKeys = alphaKeys;
                lines[1].colorGradient = gradient;

                //width
                //AnimationCurve curve = new AnimationCurve();
                //curve.AddKey(0.0f, 1.0f); curve.AddKey(1.0f, 0.0f);
                //lr[1].widthCurve = curve;
                break;
            }
            case 1:
            {
                var gradient = lines[1].colorGradient;
                var alphaKeys = gradient.alphaKeys;
                alphaKeys[0].alpha = 1f;
                alphaKeys[0].time = 0f;
                alphaKeys[1].alpha = 0.8f;
                alphaKeys[1].time = 1f;

                gradient.alphaKeys = alphaKeys;
                lines[1].colorGradient = gradient;

                alphaKeys[0].alpha = 0.8f;
                alphaKeys[0].time = 0f;
                alphaKeys[1].alpha = 0f;
                alphaKeys[1].time = 1f;

                gradient.alphaKeys = alphaKeys;
                lines[2].colorGradient = gradient;

                //width
                //AnimationCurve curve1 = new AnimationCurve();
                //curve1.AddKey(0.0f, 1.0f); curve1.AddKey(0.0f, 1.0f);
                //lr[1].widthCurve = curve1;

                //AnimationCurve curve2 = new AnimationCurve();
                //curve2.AddKey(0.0f, 1.0f); curve2.AddKey(1.0f, 0.0f);
                //lr[2].widthCurve = curve2;
                break;
            }
            case 2:
            {
                var gradient = lines[1].colorGradient;
                var alphaKeys = gradient.alphaKeys;
                alphaKeys[0].alpha = 1f;
                alphaKeys[0].time = 0f;
                alphaKeys[1].alpha = 1f;
                alphaKeys[1].time = 1f;

                gradient.alphaKeys = alphaKeys;
                lines[1].colorGradient = gradient;

                alphaKeys[0].alpha = 1f;
                alphaKeys[0].time = 0f;
                alphaKeys[1].alpha = 0.6f;
                alphaKeys[1].time = 1f;

                gradient.alphaKeys = alphaKeys;
                lines[2].colorGradient = gradient;

                alphaKeys[0].alpha = 0.6f;
                alphaKeys[0].time = 0f;
                alphaKeys[1].alpha = 0f;
                alphaKeys[1].time = 1f;

                gradient.alphaKeys = alphaKeys;
                lines[3].colorGradient = gradient;

                //width
                //AnimationCurve curve1 = new AnimationCurve();
                //curve1.AddKey(0.0f, 1.0f); curve1.AddKey(1.0f, 1.0f);
                //lr[1].widthCurve = curve1;

                //AnimationCurve curve2 = new AnimationCurve();
                //curve2.AddKey(0.0f, 1.0f); curve2.AddKey(1.0f, 1.0f);
                //lr[2].widthCurve = curve2;

                //AnimationCurve curve3 = new AnimationCurve();
                //curve3.AddKey(0.0f, 1.0f); curve3.AddKey(1.0f, 0.0f);
                //lr[3].widthCurve = curve2;
                break;
            }
        }
    }

    #endregion


    public void guidelineOff()
    {
        linesGroup.gameObject.SetActive(false);
    }

    public void guidelineOn()
    {
        linesGroup.gameObject.SetActive(true);

        refreshGuideLine();
    }

    public void setPlayer(APlayer p)
    {
        player = p;
    }

    public void setShootPosition(Vector3 pos, bool resetShootDirection)
    {
        shootPosition = pos;
        if (resetShootDirection)
            shootDirection = Vector3.up;
    }

    public void setOriginalShootPosition(Vector3 pos)
    {
        originalShootPosition = pos;
    }

    public Vector2 getShootDirection()
    {
        return shootDirection;
    }

    public Vector2 getRawShootDirection()
    {
        return rawShootDirection;
    }

    public void setShootDirectionLimitAngle(float min, float max)
    {
        minShootDirectionLimitAngle = min;
        maxShootDirectionLimitAngle = max;
    }

    public void resetShootDirectionLimitAngle()
    {
        minShootDirectionLimitAngle = defaultMinShootDirectionLimitAngle;
        maxShootDirectionLimitAngle = defaultMaxShootDirectionLimitAngle;
    }

    public void addMask(int mask)
    {
        _mask0 |= mask;
        _mask1 |= mask;
        _mask2 |= mask;
    }

    public void removeMask(int mask)
    {
        _mask0 &= ~mask;
        _mask1 &= ~mask;
        _mask2 &= ~mask;
    }

    public void setIndicatorBallPosition(Vector3 pos)
    {
        indicatorBall.position = pos;
    }

    public void setIndicatorBallActive(bool active)
    {
        indicatorBall.gameObject.SetActive(active);
    }
}