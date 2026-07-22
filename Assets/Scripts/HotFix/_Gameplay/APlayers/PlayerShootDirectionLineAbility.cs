using UnityEngine;

namespace MoreMountains
{
    public class PlayerShootDirectionLineAbility : PlayerAbility
    {
        static int MainTex = Shader.PropertyToID("_MainTex");

        Material lineMat0, lineMat1, lineMat2;
        Transform linesGroup;
        LineRenderer[] lines = new LineRenderer[4];
        Transform[] dots = new Transform[2];
        Transform indicatorBall;

        LayerMask _mask0, _hit2Mask;
        public Transform reticleTransform;

        public float distance = 5f;
        bool isLine;
        bool isOff;
        Vector3 shootPosition => _player.getWorldPosition();
        Vector3 rawShootDirection, shootDirection;
        Vector3 mouseWorldPos;

        protected override void Initialization()
        {
            base.Initialization();
            var obstacle = LayerManager.Obstacles_Mask;
            var brick = BRICK_LAYER_MASK;
            _mask0 = obstacle | brick;
            
            var t = transform;
            t.find(out indicatorBall, "IndicatorBall");
            t.find(out linesGroup, "Group");
            t.find(out lines[0], "Line0");
            t.find(out lines[1], "Line1");
            t.find(out lines[2], "Line2");
            t.find(out lines[3], "Line3");

            t.find(out dots[0], "Dot0");
            t.find(out dots[1], "Dot1");

            // lines[2].enabled = false;
            // lines[3].enabled = false;

            lines[0].positionCount = 4;
            lines[1].positionCount = 2;
            lines[2].positionCount = 2;
            lines[3].positionCount = 2;

            lineMat0 = lines[0].sharedMaterial;
            lineMat1 = lines[1].sharedMaterial;
            lineMat2 = lines[2].sharedMaterial;
        }

        protected override void HandleInput()
        {
            var pos = _inputManager.MousePosition;
            mouseWorldPos = screenToWorld(pos, false);
        }

        void updateShootDirection()
        {
            var diff = mouseWorldPos - shootPosition;
            diff.Normalize();
            var rotZ = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;
            var rotation = Quaternion.Euler(0f, 0f, rotZ - 90);
            shootDirection = rotation * Vector3.up;

            var rotationRaw = Quaternion.Euler(0f, 0f, rotZ - 90);
            rawShootDirection = rotationRaw * Vector3.up;
            // Draw.ingame.xy.Line(shootPosition, shootPosition + shootDirection * 5, Color.red);
        }


        public override void OnUpdate(float dt)
        {
            base.OnUpdate(dt);

            reticleTransform.localPosition = transform.InverseTransformPoint(mouseWorldPos);
            
            if (isOff)
                return;

            //Material Anim
            float offset = Time.time * -1f;
            lineMat0.mainTextureScale = new(1, 1);
            lineMat0.SetTextureOffset(MainTex, new(offset, 0f));
            lineMat1.SetTextureOffset(MainTex, new(offset, 0f));
            lineMat2.SetTextureOffset(MainTex, new(offset, 0f));

            refreshGuideLine();
        }

        void refreshGuideLine()
        {
            Vector2 origin = shootPosition;
            updateShootDirection();
            Vector2 up = shootDirection;
            Vector2 dir = origin - (up * -distance);

            var radius = 0.10F;
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
                        _hit2Mask = _mask0;
                    }
                    else
                    {
                        //第一次射线检测到屏幕左侧
                        _hit2Mask = _mask0;
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

        public Vector2 getShootDirection()
        {
            return shootDirection;
        }

        public Vector2 getRawShootDirection()
        {
            return rawShootDirection;
        }

        public void addMask(int mask)
        {
            _mask0 |= mask;
        }

        public void removeMask(int mask)
        {
            _mask0 &= ~mask;
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
}