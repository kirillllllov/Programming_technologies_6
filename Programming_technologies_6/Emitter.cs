using System;
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
        List<Particle> particles = new List<Particle>();
        public List<IImpactPoint> impactPoints = new List<IImpactPoint>();
        public int MousePositionX;
        public int MousePositionY;
        public float GravitationX = 0;
        public float GravitationY = 0; // пусть гравитация будет силой один пиксель за такт, нам хватит

        public void UpdateState()
        {
            foreach (var particle in particles)
            {
                particle.Life -= 1; // уменьшаю здоровье
                                    // если здоровье кончилось
                if (particle.Life < 0)
                {
                    ResetParticle(particle);
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
                    var particle = new ParticleColorful();
                    particle.FromColor = Color.White;
                    particle.ToColor = Color.FromArgb(0, Color.Black);

                    ResetParticle(particle); // добавили вызов ResetParticle

                    particles.Add(particle);
                }
                else
                {
                    break;
                }
            }
        }
        public int ParticlesCount = 500;

        // добавил новый метод, виртуальным, чтобы переопределять можно было
        public virtual void ResetParticle(Particle particle)
        {
            particle.Life = 20 + Particle.rand.Next(100);
            particle.X = MousePositionX;
            particle.Y = MousePositionY;

            var direction = (double)Particle.rand.Next(360);
            var speed = 1 + Particle.rand.Next(10);

            particle.SpeedX = (float)(Math.Cos(direction / 180 * Math.PI) * speed);
            particle.SpeedY = -(float)(Math.Sin(direction / 180 * Math.PI) * speed);

            particle.Radius = 2 + Particle.rand.Next(10);
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
