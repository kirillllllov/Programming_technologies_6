﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Programming_technologies_6.Particle;

namespace Programming_technologies_6
{
    class Emitter
    {
        public int X; // координата X центра эмиттера, будем ее использовать вместо MousePositionX
        public int Y; // соответствующая координата Y 
        public int Direction = 0; // вектор направления в градусах куда сыпет эмиттер
        public int Spreading = 360; // разброс частиц относительно Direction
        public int SpeedMin = 1; // начальная минимальная скорость движения частицы
        public int SpeedMax = 10; // начальная максимальная скорость движения частицы
        public int RadiusMin = 2; // минимальный радиус частицы
        public int RadiusMax = 10; // максимальный радиус частицы
        public int LifeMin = 20; // минимальное время жизни частицы
        public int LifeMax = 100; // максимальное время жизни частицы
        public int ParticlesPerTick = 1;

        public Color ColorFrom = Color.White; // начальный цвет частицы
        public Color ColorTo = Color.FromArgb(0, Color.Black); // конечный цвет частиц
        List<Particle> particles = new List<Particle>();
        public List<IImpactPoint> impactPoints = new List<IImpactPoint>();
        public int MousePositionX;
        public int MousePositionY;
        public float GravitationX = 0;
        public float GravitationY = 1; // пусть гравитация будет силой один пиксель за такт, нам хватит

        public virtual Particle CreateParticle()
        {
            var particle = new ParticleColorful();
            particle.FromColor = ColorFrom;
            particle.ToColor = ColorTo;

            return particle;
        }

        public void UpdateState()
        {
            int particlesToCreate = ParticlesPerTick;

            foreach (var particle in particles)
            {
                particle.Life -= 1; // уменьшаю здоровье
                                    // если здоровье кончилось
                if (particle.Life <= 0)
                {
                    if (particlesToCreate > 0)
                    {
                        /* у нас как сброс частицы равносилен созданию частицы */
                        particlesToCreate -= 1; // поэтому уменьшаем счётчик созданных частиц на 1
                        ResetParticle(particle);
                    }
                }
                else
                {
                    foreach (var point in impactPoints)
                    {
                        point.ImpactParticle(particle);
                    }

                    // это не трогаем
                    particle.SpeedX += GravitationX;
                    particle.SpeedY += GravitationY;

                    particle.X += particle.SpeedX;
                    particle.Y += particle.SpeedY;
                }
            }
            for (var i = 0; i < 10; ++i)
            {
                if (particles.Count < ParticlesCount)
                {
                    var particle = CreateParticle(); // и собственно теперь тут его вызываем
                    ResetParticle(particle);
                    particles.Add(particle);
                }
                else
                {
                    break;
                }
            }
            while (particlesToCreate >= 1)
            {
                particlesToCreate -= 1;
                var particle = CreateParticle();
                ResetParticle(particle);
                particles.Add(particle);
            }

        }
        public int ParticlesCount = 500;

        // добавил новый метод, виртуальным, чтобы переопределять можно было
        public virtual void ResetParticle(Particle particle)
        {
            particle.Life = Particle.rand.Next(LifeMin, LifeMax);

            particle.X = X;
            particle.Y = Y;

            var direction = Direction
                + (double)Particle.rand.Next(Spreading)
                - Spreading / 2;

            var speed = Particle.rand.Next(SpeedMin, SpeedMax);

            particle.SpeedX = (float)(Math.Cos(direction / 180 * Math.PI) * speed);
            particle.SpeedY = -(float)(Math.Sin(direction / 180 * Math.PI) * speed);

            particle.Radius = Particle.rand.Next(RadiusMin, RadiusMax);
        }

        public abstract class IImpactPoint
        {
            public float X; // ну точка же, вот и две координаты
            public float Y;

            // абстрактный метод с помощью которого будем изменять состояние частиц
            // например притягивать
            public abstract void ImpactParticle(Particle particle);

            // базовый класс для отрисовки точечки
            public void Render(Graphics g)
            {
                g.FillEllipse(
                        new SolidBrush(Color.Red),
                        X - 5,
                        Y - 5,
                        10,
                        10
                    );
            }
        }
        public class GravityPoint : IImpactPoint
        {
            public int Power = 100; // сила притяжения

            // а сюда по сути скопировали с минимальными правками то что было в UpdateState
            public override void ImpactParticle(Particle particle)
            {
                float gX = X - particle.X;
                float gY = Y - particle.Y;
                float r2 = (float)Math.Max(100, gX * gX + gY * gY);

                particle.SpeedX += gX * Power / r2;
                particle.SpeedY += gY * Power / r2;
            }
        }

        public class AntiGravityPoint : IImpactPoint
        {
            public int Power = 100; // сила отторжения

            // а сюда по сути скопировали с минимальными правками то что было в UpdateState
            public override void ImpactParticle(Particle particle)
            {
                float gX = X - particle.X;
                float gY = Y - particle.Y;
                float r2 = (float)Math.Max(100, gX * gX + gY * gY);

                particle.SpeedX -= gX * Power / r2; // тут минусики вместо плюсов
                particle.SpeedY -= gY * Power / r2; // и тут
            }
        }

        public class TopEmitter : Emitter
        {
            public int Width; // длина экрана

            public override void ResetParticle(Particle particle)
            {
                base.ResetParticle(particle); // вызываем базовый сброс частицы, там жизнь переопределяется и все такое

                // а теперь тут уже подкручиваем параметры движения
                particle.X = Particle.rand.Next(Width); // позиция X -- произвольная точка от 0 до Width
                particle.Y = 0;  // ноль -- это верх экрана 

                particle.SpeedY = 1; // падаем вниз по умолчанию
                particle.SpeedX = Particle.rand.Next(-2, 2); // разброс влево и вправа у частиц 
            }
        }

        public void Render(Graphics g)
        {
            foreach (var particle in particles)
            {
                particle.Draw(g);
            }
            foreach (var point in impactPoints)
            {
                point.Render(g);
            }
        }
    }
}
