using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace yoketoru_2016_6_17_2
{
    public partial class Form1 : Form
    {
        static Random rand = new Random();
        //敵の最大速度
        const float ENEMY_SPEED = 10f;
        //アイテムの最大速度
        const float ITEM_SPEED = 10f;
        //アイテムの残り数
        int iItemCount;
        enum SCENES
        {
            SC_NONE,       //無効
            SC_BOOT,       //起動
            SC_TITLE,      //タイトル
            SC_GAME,       //ゲーム中
            SC_GAMEOVER,   //ゲームオーバー
            SC_CLEAR       //クリア
        };
        /**現在のシーン*/
        SCENES nowScene = SCENES.SC_NONE;
        /**次のシーン*/
        SCENES nextScene = SCENES.SC_BOOT;

        /**敵の上限数*/
        const int ENEMY_MAX = 10;
        /**アイテム上限数*/
        const int ITEM_MAX = 10;
        /**キャラクター上限数*/
        const int CHR_MAX = 1 + ENEMY_MAX + ITEM_MAX;

        /**キャラクタータイプ*/
        enum CHRTYPE
        {
            CHRTYPE_NONE,
            CHRTYPE_PLAYER,
            CHRTYPE_ENEMY,
            CHRTYPE_ITEM,
        }

        /**キャラクタータイプ*/
        CHRTYPE[] type = new CHRTYPE[CHR_MAX];
        /** X座標*/
        float[] px = new float[CHR_MAX];
        /** Y座標*/
        float[] py = new float[CHR_MAX];
        /** X速度*/
        float[] vx = new float[CHR_MAX];
        /** Y速度*/
        float[] vy = new float[CHR_MAX];
        /** ラベル*/
        Label[] labels = new Label[CHR_MAX];

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        /** 初期化*/
        private void init()
        {
            // シーンの切り替えが必要か？
            if (nextScene == SCENES.SC_NONE)
                return; // init()をでる
            //シーンの切り替えが必要
            nowScene = nextScene;
            nextScene = SCENES.SC_NONE;

            switch(nowScene)
            {
                //
                case SCENES.SC_BOOT:
                    for(int i=0; i<CHR_MAX;i++)
                    {
                        type[i] = CHRTYPE.CHRTYPE_NONE;
                        labels[i] = new Label();
                        labels[i].Visible = false;
                        labels[i].AutoSize = true;
                        Controls.Add(labels[i]);
                    }
                    nextScene = SCENES.SC_TITLE;
                    break;

                //ゲーム開始時の初期化
                case SCENES.SC_GAME:
                    type[0] = CHRTYPE.CHRTYPE_PLAYER;
                    vx[0] = 0;
                    vy[0] = 0;
                    labels[0].Text = "(・ω・)";
                    labels[0].Visible = true;
                    px[0] = (ClientSize.Width - labels[0].Width) / 2;
                    py[0] = (ClientSize.Height - labels[0].Height) / 2;
                    labels[0].Left = (int)px[0];
                    labels[0].Top = (int)py[0];
                    iItemCount = ITEM_MAX;

                    //敵の初期化
                    for (int i = 1; i < 1 + ENEMY_MAX; i++)
                    {
                        type[i] = CHRTYPE.CHRTYPE_ENEMY;
                        vx[i] = (float)(rand.NextDouble()
                            * (2 * ENEMY_SPEED) - ENEMY_SPEED);
                        vy[i] = (float)(rand.NextDouble() 
                            * (2 * ENEMY_SPEED) - ENEMY_SPEED);
                        labels[i].Text = "●";
                        px[i] = rand.Next(ClientSize.Width - labels[i].Width);
                        py[i] = rand.Next(ClientSize.Height - labels[i].Height);
                    }
                    //アイテムの初期化
                    for (int i = 1+ENEMY_MAX; i < CHR_MAX; i++)
                    {
                        type[i] = CHRTYPE.CHRTYPE_ITEM;
                        vx[i] = (float)(rand.NextDouble()
                            * (2 * ITEM_SPEED) - ITEM_SPEED);
                        vy[i] = (float)(rand.NextDouble()
                            * (2 * ITEM_SPEED) - ITEM_SPEED);
                        labels[i].Text = "★";
                        px[i] = rand.Next(ClientSize.Width - labels[i].Width);
                        py[i] = rand.Next(ClientSize.Height - labels[i].Height);
                    }

                    break;
            }
        }

        /** 更新処理*/
        private void update()
        {
            switch (nowScene)
            {
                case SCENES.SC_TITLE:
                    if ((Control.MouseButtons & MouseButtons.Left)
                        == MouseButtons.Left)
                    {
                        // 左クリックされた
                        nextScene = SCENES.SC_GAME;
                    }
                    break;
                case SCENES.SC_GAME:
                    updateGame();
                    break;
                case SCENES.SC_CLEAR:
                case SCENES.SC_GAMEOVER:
                    if ((Control.MouseButtons & MouseButtons.Left)
                        == MouseButtons.Left)
                    {
                        // 左クリックされた
                        nextScene = SCENES.SC_TITLE;
                    }
                    break;
            }
        }
        /** */
        private void updateGame()
        {
            for(int i=0;i<CHR_MAX;i++)
            {
                switch(type[i])
                {
                    case CHRTYPE.CHRTYPE_PLAYER:
                        updatePlayer(i);
                        break;
                    case CHRTYPE.CHRTYPE_ENEMY:
                        updateEnemy(i);
                        break;
                    case CHRTYPE.CHRTYPE_ITEM:
                        updateItem(i);
                        break;
                }

            }
        }

        /** 敵の更新処理*/
        private void updateEnemy(int i)
        {
            constantMove(i);
            if(hitPlayer(i))
            {
                nextScene = SCENES.SC_GAMEOVER;
            }
        }

        /** アイテムの更新処理*/
        private void updateItem(int i)
        {
            constantMove(i);
            if(hitPlayer(i))
            {
                //アイテムを消す
                type[i] = CHRTYPE.CHRTYPE_NONE;
                //クリアチェック
                iItemCount--;
                if(iItemCount<=0)
                {
                    //アイテム全部取得
                    nextScene = SCENES.SC_CLEAR;
                }
            }
        }

        /**
         * プレイヤーとiがぶつかっているか？
         * @return bool true = ぶつかっている / false = ぶつかっていない
         */
        private bool hitPlayer(int i)
        {
            if((px[0] < labels[i].Right)
                && (px[i] < labels[0].Right)
                && (py[0] < labels[i].Top)
                && (py[i] < labels[0].Bottom)
                )
            {
                return true;
            }

            return false; 
            
            
        }
        /** 等速直線運動でキャラを動かす*/
        private void constantMove(int i)
        {
            px[i] += vx[i];
            py[i] += vy[i];
            /** 左跳ね返り*/
            if(px[i] < 0)
            {
                vx[i] = Math.Abs(vx[i]);
            }
            /** 右跳ね返り*/
            else if(px[i]>ClientSize.Width-labels[i].Width)
            {
                vx[i] = -Math.Abs(vx[i]);
            }
            /** 上跳ね返り*/
            if (py[i] < 0)
            {
                vy[i] = Math.Abs(vy[i]);
            }
            /** 下跳ね返り*/
            else if (py[i] > ClientSize.Height - labels[i].Height)
            {
                vy[i] = -Math.Abs(vy[i]);
            }
        }

        /** プレイヤーの更新処理*/
        private void updatePlayer(int i)
        {
            Point cpos;
            cpos = PointToClient(MousePosition);
            px[0] = cpos.X - labels[0].Width / 2;
            py[0] = cpos.Y - labels[0].Height / 2;
            
            

        }
        
        /** 描画*/
        private void render()
        {
            switch (nowScene)
            {
                case SCENES.SC_TITLE:
                    Text = "TITLE";
                    break;
                case SCENES.SC_GAME:
                    Text = "GAME";
                    for (int i = 0; i < CHR_MAX; i++)
                    {
                        if (type[i] != CHRTYPE.CHRTYPE_NONE)
                        {
                            labels[i].Visible = true;
                            labels[i].Left = (int)px[i];
                            labels[i].Top = (int)py[i];
                        }
                        else
                        {
                            labels[i].Visible = false;
                        }
                    }
                    break;
                case SCENES.SC_GAMEOVER:
                    Text = "GAMEOVER";
                    break;
                case SCENES.SC_CLEAR:
                    Text = "CLEAR";
                    break;
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            init();
            update();
            render();
        }
    }
}
