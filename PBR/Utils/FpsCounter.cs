using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PBR.Utils;

internal class FpsCounter
{
    private int _frameCnt;
    private double _elapsedFramesTimeSec;
    private double _fps;

    public double Fps => _fps;

    public void Update(GameTime gameTime)
    {
        _frameCnt++;
        _elapsedFramesTimeSec += gameTime.ElapsedGameTime.TotalSeconds;

        if (_elapsedFramesTimeSec < 0.5) return;

        _fps = _frameCnt / _elapsedFramesTimeSec;
        _frameCnt = 0;
        _elapsedFramesTimeSec = 0;
    }
}