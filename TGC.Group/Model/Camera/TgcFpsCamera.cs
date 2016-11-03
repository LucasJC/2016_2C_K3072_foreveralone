using System;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.DirectX;
using Microsoft.DirectX.DirectInput;
using TGC.Core.Camara;
using TGC.Core.Direct3D;
using TGC.Core.Input;
using TGC.Core.Utils;
using TGC.Group.Model;
using TGC.Core.Geometry;
using TGC.Core.SceneLoader;

namespace TGC.Examples.Camara
{
    /// <summary>
    ///     Camara en primera persona que utiliza matrices de rotacion, solo almacena las rotaciones en updown y costados.
    ///     Ref: http://www.riemers.net/eng/Tutorials/XNA/Csharp/Series4/Mouse_camera.php
    ///     Autor: Rodrigo Garcia.
    /// </summary>
    public class TgcFpsCamera : TgcCamera
    {
        public float FixedHeight = 15;
        public float MapXLimit = 0;
        public float MapXNegLimit = 0;
        public float MapZLimit = 0;
        public float MapZNegLimit = 0;

        private readonly Point mouseCenter; //Centro de mause 2D para ocultarlo.

        //Se mantiene la matriz rotacion para no hacer este calculo cada vez.
        private Matrix cameraRotation;

        //Direction view se calcula a partir de donde se quiere ver con la camara inicialmente. por defecto se ve en -Z.
        private Vector3 directionView;

        //No hace falta la base ya que siempre es la misma, la base se arma segun las rotaciones de esto costados y updown.
        private float leftrightRot;
        private float updownRot;

        private bool lockCam;
        private Vector3 positionEye;

        // bloquea el movimiento
        public bool DisableMovement { get; set; } = false;

        private bool Jumping = false;
        private bool Falling = false;
        private float JumpingTime = 0f;

        private Player player1;

        private float MovementSpeed;

        public float RunningSpeed { get; set; }
        public float WalkingSpeed { get; set; }
        public float RotationSpeed { get; set; }
        public float Gravity { get; set; }
        public float MouseRotationSpeed { get; set; }
        public float JumpSpeed { get; set; }

        //para colisiones
        public Vector3 PreviousPosition { get; set; }
        public Vector3 PreviousLookAt { get; set; }
        public Vector3 PreviousUpVector { get; set; }
        public bool Collisioned { get; set; } = false;
        public TgcBox CameraBox { get; set; } = TgcBox.fromExtremes(new Vector3(0, 0, 0), new Vector3(2, 2, 2));
        public Key LastMovementKey;

        private TgcMesh axe;

        public TgcFpsCamera(TgcD3dInput input)
        {
            Input = input;
            positionEye = new Vector3(0, 10, 0);
            mouseCenter = new Point(
                D3DDevice.Instance.Device.Viewport.Width / 2,
                D3DDevice.Instance.Device.Viewport.Height / 2);
            RotationSpeed = 0.0015f;
            MouseRotationSpeed = 0.01f;
            WalkingSpeed = 75f;
            RunningSpeed = 150f;
            MovementSpeed = WalkingSpeed;
            JumpSpeed = 200f;
            directionView = new Vector3(0, 0, -1);
            leftrightRot = 0;
            updownRot = 0;
            cameraRotation = Matrix.RotationX(updownRot) * Matrix.RotationY(leftrightRot);
            Gravity = 0.025f;

            this.PreviousPosition = positionEye;
            this.PreviousLookAt = directionView;
            this.PreviousUpVector = DEFAULT_UP_VECTOR;
        }

        public TgcFpsCamera(Player player1, TgcMesh axe, TgcD3dInput input, float mapXLimit, float mapXNegLimit, float mapZLimit, float mapZNegLimit) : this(input)
        {
            this.player1 = player1;
            this.axe = axe;
            this.MapXLimit = mapXLimit;
            this.MapXNegLimit = mapXNegLimit;
            this.MapZLimit = mapZLimit;
            this.MapZNegLimit = mapZNegLimit;
            this.lockCam = true;
            Cursor.Hide();
        }

        public TgcFpsCamera(Vector3 positionEye, TgcD3dInput input) : this(input)
        {
            this.positionEye = positionEye;
        }

        public TgcFpsCamera(Vector3 positionEye, float moveSpeed, float jumpSpeed, TgcD3dInput input)
            : this(positionEye, input)
        {
            MovementSpeed = moveSpeed;
            JumpSpeed = jumpSpeed;
        }

        public TgcFpsCamera(Vector3 positionEye, float moveSpeed, float jumpSpeed, float rotationSpeed,
            TgcD3dInput input)
            : this(positionEye, moveSpeed, jumpSpeed, input)
        {
            RotationSpeed = rotationSpeed;
        }

        private TgcD3dInput Input { get; }

        public bool LockCam
        {
            get { return lockCam; }
            set
            {
                if (!lockCam && value)
                {
                    Cursor.Position = mouseCenter;

                    Cursor.Hide();
                }
                if (lockCam && !value)
                    Cursor.Show();
                lockCam = value;
            }
        }

        /// <summary>
        ///     Cuando se elimina esto hay que desbloquear la camera.
        /// </summary>
        ~TgcFpsCamera()
        {
            LockCam = false;
        }
        public override void UpdateCamera(float elapsedTime)
        {
            if (!DisableMovement)
            {
                //guardo pos anterior
                this.PreviousPosition = this.Position;
                this.PreviousLookAt = this.LookAt;
                this.PreviousUpVector = this.UpVector;

                var JumpTime = 3;
                var moveVector = new Vector3(0, 0, 0);

                if (player1.Stamina > 0 && Input.keyDown(Key.LeftShift))
                {
                    this.MovementSpeed = RunningSpeed;
                }
                else
                {
                    this.MovementSpeed = WalkingSpeed;
                }

                player1.Moving = false;

                //Forward
                if (Input.keyDown(Key.W))
                {
                    if (Collisioned && LastMovementKey == Key.W)
                    {
                        //no se puede mover en esta dirección
                    }
                    else
                    {
                        moveVector += new Vector3(0, 0, -1) * MovementSpeed;
                        player1.Moving = true;
                        LastMovementKey = Key.W;
                    }
                }

                //Backward
                if (Input.keyDown(Key.S))
                {
                    if (Collisioned && LastMovementKey == Key.S)
                    {
                        //no se puede mover en esta dirección
                    }
                    else
                    {
                        moveVector += new Vector3(0, 0, 1) * MovementSpeed;
                        player1.Moving = true;
                        LastMovementKey = Key.S;
                    }
                }

                //Strafe right
                if (Input.keyDown(Key.D))
                {
                    if (Collisioned && LastMovementKey == Key.D)
                    {
                        //no se puede mover en esta dirección
                    }
                    else
                    {
                        moveVector += new Vector3(-1, 0, 0) * MovementSpeed;
                        player1.Moving = true;
                        LastMovementKey = Key.D;
                    }
                }

                //Strafe left
                if (Input.keyDown(Key.A))
                {
                    if (Collisioned && LastMovementKey == Key.A)
                    {
                        //no se puede mover en esta dirección
                    }
                    else
                    {
                        moveVector += new Vector3(1, 0, 0) * MovementSpeed;
                        player1.Moving = true;
                        LastMovementKey = Key.A;
                    }
                }

                //Jump
                if (Input.keyPressed(Key.Space))
                {
                    if (!Jumping)
                    {
                        Jumping = true;
                    }
                }
                if (Jumping)
                {
                    if (JumpingTime < JumpTime) moveVector += new Vector3(0, 1, 0) * JumpSpeed;
                    if (JumpingTime >= JumpTime)
                    {
                        Jumping = false;
                        Falling = true;
                        JumpingTime = 0;
                    }
                    JumpingTime += elapsedTime * 10;
                }

                if (Input.keyPressed(Key.L) || Input.keyPressed(Key.Escape))
                {
                    LockCam = !lockCam;
                }

                //Solo rotar si se esta aprentando el boton izq del mouse
                if (lockCam || Input.buttonDown(TgcD3dInput.MouseButtons.BUTTON_LEFT))
                {
                    leftrightRot -= -Input.XposRelative * MouseRotationSpeed;
                    updownRot -= Input.YposRelative * MouseRotationSpeed;
                    //Se actualiza matrix de rotacion, para no hacer este calculo cada vez y solo cuando en verdad es necesario.
                    cameraRotation = Matrix.RotationX(updownRot) * Matrix.RotationY(leftrightRot);
                }

                if (lockCam)
                    Cursor.Position = mouseCenter;

                //Calculamos la nueva posicion del ojo segun la rotacion actual de la camara.
                var cameraRotatedPositionEye = Vector3.TransformNormal(moveVector * elapsedTime, cameraRotation);
                positionEye += cameraRotatedPositionEye;

                //IMPORTANTE - esta parte hardcodea los límites del mapa -- hay que rehacerla de alguna manera más copada
                if (Falling && positionEye.Y > FixedHeight)
                {
                    positionEye.Y = positionEye.Y - JumpSpeed * elapsedTime + 0.5f * Gravity * elapsedTime * elapsedTime;

                    if (positionEye.Y <= FixedHeight) Falling = false;

                }
                else if (!Jumping && positionEye.Y > FixedHeight)
                {
                    positionEye.Y = FixedHeight;
                }
                else if (positionEye.Y < FixedHeight)
                {
                    positionEye.Y = FixedHeight;
                }

                if (MapXLimit != 0 && positionEye.X >= MapXLimit * .97f) positionEye.X = MapXLimit * .97f;
                if (MapXNegLimit != 0 && positionEye.X <= MapXNegLimit * .97f) positionEye.X = MapXNegLimit * .97f;
                if (MapZLimit != 0 && positionEye.Z >= MapZLimit * .97f) positionEye.Z = MapZLimit * .97f;
                if (MapZNegLimit != 0 && positionEye.Z <= MapZNegLimit * .97f) positionEye.Z = MapZNegLimit * .97f;

                //Calculamos el target de la camara, segun su direccion inicial y las rotaciones en screen space x,y.
                var cameraRotatedTarget = Vector3.TransformNormal(directionView, cameraRotation);
                var cameraFinalTarget = positionEye + cameraRotatedTarget;

                var cameraOriginalUpVector = DEFAULT_UP_VECTOR;
                var cameraRotatedUpVector = Vector3.TransformNormal(cameraOriginalUpVector, cameraRotation);

                base.SetCamera(positionEye, cameraFinalTarget, cameraRotatedUpVector);

                axe.Position = new Vector3(positionEye.X + 2, positionEye.Y - 2, positionEye.Z);
                axe.Transform = Matrix.RotationX(updownRot) * Matrix.RotationY(leftrightRot) * Matrix.Translation(axe.Position) * Matrix.Scaling(axe.Scale);
            }
        }

        /// <summary>
        ///     cancela el movimiento de cámara ya que el usuario murió
        /// </summary>
        internal void gameOver()
        {
            this.lockCam = false;
            this.DisableMovement = true;
        }

        /// <summary>
        ///     se hace override para actualizar las posiones internas, estas seran utilizadas en el proximo update.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="directionView"> debe ser normalizado.</param>
        public override void SetCamera(Vector3 position, Vector3 directionView)
        {
            positionEye = position;
            this.directionView = directionView;
        }

        public void render()
        {
            axe.render();
        }
    }
}