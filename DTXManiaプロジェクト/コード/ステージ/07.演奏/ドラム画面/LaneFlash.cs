﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DTXMania;
using FDK;

namespace TJAPlayer3
{
    /// <summary>
    /// レーンフラッシュのクラス。
    /// </summary>
    class LaneFlash : CActivity
    {

        public LaneFlash(ref CTexture texture, int player)
        {
            Texture = texture;
            Player = player;
            base.b活性化してない = true;
        }

        public void Start()
        {
            Counter = new CCounter(0, 100, 2, CDTXMania.Timer);
        }

        public override void On活性化()
        {
            Counter = new CCounter();
            base.On活性化();
        }

        public override void On非活性化()
        {
            Counter = null;
            base.On非活性化();
        }

        public override int On進行描画()
        {
            if (Texture == null || Counter == null) return base.On進行描画();
            if (!Counter.b停止中)
            {
                Counter.t進行();
                if (Counter.b終了値に達した) Counter.t停止();
            }
            int opacity = (((150 - Counter.n現在の値) * 255) / 100);
            Texture.n透明度 = opacity;
            Texture.t2D描画(CDTXMania.app.Device, CDTXMania.Skin.nScrollFieldBGX[Player], CDTXMania.Skin.nScrollFieldY[Player]);
            return base.On進行描画();
        }

        private CTexture Texture;
        private CCounter Counter;
        private int Player;
    }
}
