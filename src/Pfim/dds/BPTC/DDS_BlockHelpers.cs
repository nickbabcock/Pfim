using System.Diagnostics;

namespace Pfim.dds.bptc
{
    internal static class DDS_BlockHelpers
    {
        const double OneThird = 1f / 3f;
        const double TwoThirds = 2f / 3f;

        #region Block Compression
        #region RGB DXT
        /// <summary>
        /// This region contains stuff adpated/taken from the DirectXTex project: https://github.com/Microsoft/DirectXTex
        /// Things needed to be in the range 0-1 instead of 0-255, hence new struct etc
        /// </summary>
        [DebuggerDisplay("R:{r}, G:{g}, B:{b}, A:{a}")]
        internal struct RGBColour
        {
            public float r, g, b, a;

            public RGBColour(float red, float green, float blue, float alpha)
            {
                r = red;
                g = green;
                b = blue;
                a = alpha;
            }

            public override string ToString()
            {
                return $"{r.ToString("F6")} {g.ToString("F6")} {b.ToString("F6")} {a.ToString("F6")}";
            }
        }

        internal static float[] pC3 = { 1f, 1f / 2f, 0f };
        internal static float[] pD3 = { 0f, 1f / 2f, 1f };

        internal static float[] pC4 = { 1f, 2f / 3f, 1f / 3f, 0f };
        internal static float[] pD4 = { 0f, 1f / 3f, 2f / 3f, 1f };

        internal static RGBColour[] OptimiseRGB_BC67(RGBColour[] Colour, int uSteps, int np, int[] pixelIndicies)
        {
            float[] pC = uSteps == 3 ? pC3 : pC4;
            float[] pD = uSteps == 3 ? pD3 : pD4;

            // Find min max
            RGBColour X = new RGBColour(1f, 1f, 1f, 0f);
            RGBColour Y = new RGBColour(0f, 0f, 0f, 0f);

            for (int i = 0; i < np; i++)
            {
                RGBColour current = Colour[pixelIndicies[i]];

                // X = min, Y = max
                if (current.r < X.r)
                    X.r = current.r;

                if (current.g < X.g)
                    X.g = current.g;

                if (current.b < X.b)
                    X.b = current.b;


                if (current.r > Y.r)
                    Y.r = current.r;

                if (current.g > Y.g)
                    Y.g = current.g;

                if (current.b > Y.b)
                    Y.b = current.b;
            }


            // Diagonal axis - starts with difference between min and max
            RGBColour diag = new RGBColour()
            {
                r = Y.r - X.r,
                g = Y.g - X.g,
                b = Y.b - X.b
            };
            float fDiag = diag.r * diag.r + diag.g * diag.g + diag.b * diag.b;
            if (fDiag < 1.175494351e-38F)
                return new RGBColour[] { X, Y };

            float FdiagInv = 1f / fDiag;

            RGBColour Dir = new RGBColour()
            {
                r = diag.r * FdiagInv,
                g = diag.g * FdiagInv,
                b = diag.b * FdiagInv
            };

            RGBColour Mid = new RGBColour()
            {
                r = (X.r + Y.r) * 0.5f,
                g = (X.g + Y.g) * 0.5f,
                b = (X.b + Y.b) * 0.5f
            };
            float[] fDir = new float[4];

            for (int i = 0; i < np; i++)
            {
                var current = Colour[pixelIndicies[i]];

                RGBColour pt = new RGBColour()
                {
                    r = Dir.r * (current.r - Mid.r),
                    g = Dir.g * (current.g - Mid.g),
                    b = Dir.b * (current.b - Mid.b)
                };
                float f = 0;
                f = pt.r + pt.g + pt.b;
                fDir[0] += f * f;

                f = pt.r + pt.g - pt.b;
                fDir[1] += f * f;

                f = pt.r - pt.g + pt.b;
                fDir[2] += f * f;

                f = pt.r - pt.g - pt.b;
                fDir[3] += f * f;
            }

            float fDirMax = fDir[0];
            int iDirMax = 0;
            for (int iDir = 1; iDir < 4; iDir++)
            {
                if (fDir[iDir] > fDirMax)
                {
                    fDirMax = fDir[iDir];
                    iDirMax = iDir;
                }
            }

            if ((iDirMax & 2) != 0)
            {
                float f = X.g;
                X.g = Y.g;
                Y.g = f;
            }

            if ((iDirMax & 1) != 0)
            {
                float f = X.b;
                X.b = Y.b;
                Y.b = f;
            }

            if (fDiag < 1f / 4096f)
                return new RGBColour[] { X, Y };

            // newtons method for local min of sum of squares error.
            float fsteps = uSteps - 1;
            for (int iteration = 0; iteration < 8; iteration++)
            {
                RGBColour[] pSteps = new RGBColour[4];

                for (int iStep = 0; iStep < uSteps; iStep++)
                {
                    pSteps[iStep].r = X.r * pC[iStep] + Y.r * pD[iStep];
                    pSteps[iStep].g = X.g * pC[iStep] + Y.g * pD[iStep];
                    pSteps[iStep].b = X.b * pC[iStep] + Y.b * pD[iStep];
                }


                // colour direction
                Dir.r = Y.r - X.r;
                Dir.g = Y.g - X.g;
                Dir.b = Y.b - X.b;

                float fLen = Dir.r * Dir.r + Dir.g * Dir.g + Dir.b * Dir.b;

                if (fLen < (1f / 4096f))
                    break;

                float fScale = fsteps / fLen;
                Dir.r *= fScale;
                Dir.g *= fScale;
                Dir.b *= fScale;

                // Evaluate function and derivatives
                float d2X = 0, d2Y = 0;
                RGBColour dX, dY;
                dX = new RGBColour();
                dY = new RGBColour();

                for (int i = 0; i < np; i++)
                {
                    RGBColour current = Colour[pixelIndicies[i]];

                    float fDot = 
                        (current.r - X.r) * Dir.r + 
                        (current.g - X.g) * Dir.g + 
                        (current.b - X.b) * Dir.b;


                    int iStep = 0;
                    if (fDot <= 0f)
                        iStep = 0;

                    if (fDot >= fsteps)
                        iStep = uSteps - 1;
                    else
                        iStep = (int)(fDot + .5f);


                    RGBColour diff = new RGBColour()
                    {
                        r = pSteps[iStep].r - current.r,
                        g = pSteps[iStep].g - current.g,
                        b = pSteps[iStep].b - current.b
                    };


                    float fC = pC[iStep] * (1f / 8f);
                    float fD = pD[iStep] * (1f / 8f);

                    d2X += fC * pC[iStep];
                    dX.r += fC * diff.r;
                    dX.g += fC * diff.g;
                    dX.b += fC * diff.b;

                    d2Y += fD * pD[iStep];
                    dY.r += fD * diff.r;
                    dY.g += fD * diff.g;
                    dY.b += fD * diff.b;
                }

                // Move endpoints
                if (d2X > 0f)
                {
                    float f = -1f / d2X;
                    X.r += dX.r * f;
                    X.g += dX.g * f;
                    X.b += dX.b * f;
                }

                if (d2Y > 0f)
                {
                    float f = -1f / d2Y;
                    Y.r += dY.r * f;
                    Y.g += dY.g * f;
                    Y.b += dY.b * f;
                }

                const float fEpsilon = (0.25f / 64.0f) * (0.25f / 64.0f);
                if ((dX.r * dX.r < fEpsilon) && (dX.g * dX.g < fEpsilon) && (dX.b * dX.b < fEpsilon) &&
                    (dY.r * dY.r < fEpsilon) && (dY.g * dY.g < fEpsilon) && (dY.b * dY.b < fEpsilon))
                {
                    break;
                }
            }

            return new RGBColour[] { X, Y };
        }


        internal static RGBColour[] OptimiseRGBA_BC67(RGBColour[] Colour, int uSteps, int np, int[] pixelIndicies)
        {
            float[] pC = uSteps == 3 ? pC3 : pC4;
            float[] pD = uSteps == 3 ? pD3 : pD4;

            // Find min max
            RGBColour X = new RGBColour(1f, 1f, 1f, 1f);
            RGBColour Y = new RGBColour();

            for (int i = 0; i < np; i++)
            {
                RGBColour current = Colour[pixelIndicies[i]];

                // X = min, Y = max
                if (current.r < X.r)
                    X.r = current.r;

                if (current.g < X.g)
                    X.g = current.g;

                if (current.b < X.b)
                    X.b = current.b;

                if (current.a < X.a)
                    X.a = current.a;


                if (current.r > Y.r)
                    Y.r = current.r;

                if (current.g > Y.g)
                    Y.g = current.g;

                if (current.b > Y.b)
                    Y.b = current.b;

                if (current.a > Y.a)
                    Y.a = current.a;
            }

            // Diagonal axis - starts with difference between min and max
            RGBColour diag = new RGBColour()
            {
                r = Y.r - X.r,
                g = Y.g - X.g,
                b = Y.b - X.b,
                a = Y.a - X.a
            };
            float fDiag = diag.r * diag.r + diag.g * diag.g + diag.b * diag.b + diag.a * diag.a;
            if (fDiag < 1.175494351e-38F)
                return new RGBColour[] { X, Y };

            float FdiagInv = 1f / fDiag;

            RGBColour Dir = new RGBColour()
            {
                r = diag.r * FdiagInv,
                g = diag.g * FdiagInv,
                b = diag.b * FdiagInv,
                a = diag.a * FdiagInv
            };
            RGBColour Mid = new RGBColour()
            {
                r = (X.r + Y.r) * 0.5f,
                g = (X.g + Y.g) * 0.5f,
                b = (X.b + Y.b) * 0.5f,
                a = (X.a + Y.a) * 0.5f
            };
            float[] fDir = new float[8];

            for (int i = 0; i < np; i++)
            {
                var current = Colour[pixelIndicies[i]];

                RGBColour pt = new RGBColour()
                {
                    r = Dir.r * (current.r - Mid.r),
                    g = Dir.g * (current.g - Mid.g),
                    b = Dir.b * (current.b - Mid.b),
                    a = Dir.a * (current.a - Mid.a)
                };
                float f = 0;
                f = pt.r + pt.g + pt.b + pt.a;   fDir[0] += f * f;
                f = pt.r + pt.g + pt.b - pt.a;   fDir[1] += f * f;
                f = pt.r + pt.g - pt.b + pt.a;   fDir[2] += f * f;
                f = pt.r + pt.g - pt.b - pt.a;   fDir[3] += f * f;
                f = pt.r - pt.g + pt.b + pt.a;   fDir[4] += f * f;
                f = pt.r - pt.g + pt.b - pt.a;   fDir[5] += f * f;
                f = pt.r - pt.g - pt.b + pt.a;   fDir[6] += f * f;
                f = pt.r - pt.g - pt.b - pt.a;   fDir[7] += f * f;
            }

            float fDirMax = fDir[0];
            int iDirMax = 0;
            for (int iDir = 1; iDir < 8; iDir++)
            {
                if (fDir[iDir] > fDirMax)
                {
                    fDirMax = fDir[iDir];
                    iDirMax = iDir;
                }
            }

            if ((iDirMax & 4) != 0)
            {
                float f = X.g;
                X.g = Y.g;
                Y.g = f;
            }

            if ((iDirMax & 2) != 0)
            {
                float f = X.b;
                X.b = Y.b;
                Y.b = f;
            }

            if ((iDirMax & 1) != 0)
            {
                float f = X.a;
                X.a = Y.a;
                Y.a = f;
            }

            if (fDiag < 1f / 4096f)
                return new RGBColour[] { X, Y };


            // newtons method for local min of sum of squares error.
            float fsteps = uSteps - 1;
            float err = float.MaxValue;
            for (int iteration = 0; iteration < 8 && err > 0f; iteration++)
            {
                RGBColour[] pSteps = new RGBColour[DX10_Helpers.BC7_MAX_INDICIES];

                for (int iStep = 0; iStep < uSteps; iStep++)
                {
                    pSteps[iStep].r = X.r * pC[iStep] + Y.r * pD[iStep];
                    pSteps[iStep].g = X.g * pC[iStep] + Y.g * pD[iStep];
                    pSteps[iStep].b = X.b * pC[iStep] + Y.b * pD[iStep];
                    pSteps[iStep].a = X.a * pC[iStep] + Y.a * pD[iStep];
                }


                // colour direction
                Dir.r = Y.r - X.r;
                Dir.g = Y.g - X.g;
                Dir.b = Y.b - X.b;
                Dir.a = Y.a - X.a;

                float fLen = Dir.r * Dir.r + Dir.g * Dir.g + Dir.b * Dir.b + Dir.a * Dir.a;

                if (fLen < (1f / 4096f))
                    break;

                float fScale = fsteps / fLen;
                Dir.r *= fScale;
                Dir.g *= fScale;
                Dir.b *= fScale;
                Dir.a *= fScale;

                // Evaluate function and derivatives
                float d2X = 0, d2Y = 0;
                RGBColour dX, dY;
                dX = new RGBColour();
                dY = new RGBColour();

                for (int i = 0; i < np; i++)
                {
                    RGBColour current = Colour[pixelIndicies[i]];

                    float fDot =
                        (current.r - X.r) * Dir.r +
                        (current.g - X.g) * Dir.g +
                        (current.b - X.b) * Dir.b +
                        (current.a - X.a) * Dir.a;

                    int iStep = 0;
                    if (fDot <= 0f)
                        iStep = 0;
                    else if (fDot >= fsteps)
                        iStep = uSteps - 1;
                    else
                        iStep = (int)(fDot + .5f);

                    RGBColour diff = new RGBColour()
                    {
                        r = pSteps[iStep].r - current.r,
                        g = pSteps[iStep].g - current.g,
                        b = pSteps[iStep].b - current.b,
                        a = pSteps[iStep].a - current.a
                    };
                    float fC = pC[iStep] * 1f / 8f;
                    float fD = pD[iStep] * 1f / 8f;

                    d2X += fC * pC[iStep];
                    dX.r += fC * diff.r;
                    dX.g += fC * diff.g;
                    dX.b += fC * diff.b;
                    dX.a += fC * diff.a;

                    d2Y += fD * pD[iStep];
                    dY.r += fD * diff.r;
                    dY.g += fD * diff.g;
                    dY.b += fD * diff.b;
                    dY.a += fD * diff.a;
                }

                // Move endpoints
                if (d2X > 0f)
                {
                    float f = -1f / d2X;
                    X.r += dX.r * f;
                    X.g += dX.g * f;
                    X.b += dX.b * f;
                    X.a += dX.a * f;
                }

                if (d2Y > 0f)
                {
                    float f = -1f / d2Y;
                    Y.r += dY.r * f;
                    Y.g += dY.g * f;
                    Y.b += dY.b * f;
                    Y.a += dY.a * f;
                }

                float fEpsilon = (0.25f / 64.0f) * (0.25f / 64.0f);
                if ((dX.r * dX.r < fEpsilon) && (dX.g * dX.g < fEpsilon) && (dX.b * dX.b < fEpsilon) && (dX.a * dX.a < fEpsilon) &&
                    (dY.r * dY.r < fEpsilon) && (dY.g * dY.g < fEpsilon) && (dY.b * dY.b < fEpsilon) && (dY.a * dY.a < fEpsilon))
                {
                    break;
                }
            }

            return new RGBColour[] { X, Y };
        }

        #endregion RGB DXT
        #endregion Block Compression
    }
}
