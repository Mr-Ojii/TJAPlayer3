using FDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace TJAPlayer3
{
	class TextureLoader
	{
		const string BASE = @"Graphics\";

		// Stage
		const string TITLE = @"1_Title\";
		const string CONFIG = @"2_Config\";
		const string SONGSELECT = @"3_SongSelect\";
		const string SONGLOADING = @"4_SongLoading\";
		const string GAME = @"5_Game\";
		const string RESULT = @"6_Result\";
		const string EXIT = @"7_Exit\";

		// InSongSelect
		const string DIFFICULITY = @"1_Difficulty_Select\";
		const string BOX_CENTER = @"2_Box_Center\";

		// InGame
		const string CHARA = @"1_Chara\";
		const string DANCER = @"2_Dancer\";
		const string MOB = @"3_Mob\";
		const string COURSESYMBOL = @"4_CourseSymbol\";
		const string BACKGROUND = @"5_Background\";
		const string TAIKO = @"6_Taiko\";
		const string GAUGE = @"7_Gauge\";
		const string FOOTER = @"8_Footer\";
		const string END = @"9_End\";
		const string EFFECTS = @"10_Effects\";
		const string BALLOON = @"11_Balloon\";
		const string LANE = @"12_Lane\";
		const string GENRE = @"13_Genre\";
		const string GAMEMODE = @"14_GameMode\";
		const string FAILED = @"15_Failed\";
		const string RUNNER = @"16_Runner\";
		const string PUCHICHARA = @"18_PuchiChara\";
		const string TRAINING = @"19_Training\";
		const string DANC = @"17_DanC\";

		// InGame_Effects
		const string FIRE = @"Fire\";
		const string HIT = @"Hit\";
		const string ROLL = @"Roll\";
		const string SPLASH = @"Splash\";


		public TextureLoader()
		{
			// コンストラクタ

		}

		internal CTexture TxC(string FileName)
		{
			return TJAPlayer3.tテクスチャの生成(CSkin.Path(BASE + FileName));
		}
		internal CTexture TxCGen(string FileName)
		{
			return TJAPlayer3.tテクスチャの生成(CSkin.Path(BASE + GAME + GENRE + FileName + ".png"));
		}

		public void LoadTexture()
		{
			TitleTextureKey[] titleTextureKey = new TitleTextureKey[3];
			#region 共通
			Tile_Black = TJAPlayer3.ColorTexture(Brushes.Black);
			Menu_Title = TxC(@"Menu_Title.png");
			Menu_Highlight = TxC(@"Menu_Highlight.png");
			Enum_Song = TxC(@"Enum_Song.png");
			Scanning_Loudness = TxC(@"Scanning_Loudness.png");
			Overlay = TxC(@"Overlay.png");
			Overlay_Offline = TxC(@"Overlay_Offline.png");
			Overlay_Online = TxC(@"Overlay_Online.png");
			Crown_t = TxC(@"Crown.png");
			DanC_Crown_t = TxC(@"DanC_Crown.png");
			NamePlate = new CTexture[2];
			NamePlate[0] = TxC(@"1P_NamePlate.png");
			NamePlate[1] = TxC(@"2P_NamePlate.png");
			Difficulty_Icons = TxC(@"Difficulty Icons.png");
			#endregion
			#region 1_タイトル画面
			Title_Background = TxC(TITLE + @"Background.png");
			Title_Menu = TxC(TITLE + @"Menu.png");
			Title_AcBar = TxC(TITLE + @"ActiveBar.png");
			Title_InBar = TxC(TITLE + @"InactiveBar.png");
			titleTextureKey[0] = this.ttk曲名テクスチャを生成する("演奏ゲーム", Color.White, Color.SaddleBrown);
			titleTextureKey[1] = this.ttk曲名テクスチャを生成する("コンフィグ", Color.White, Color.SaddleBrown);
			titleTextureKey[2] = this.ttk曲名テクスチャを生成する("やめる", Color.White, Color.SaddleBrown);
			for (int i = 0; i < Title_Txt.Length; i++)
			{
				Title_Txt[i] = GenerateTitleTexture(titleTextureKey[i]);
			}
			#endregion

			#region 2_コンフィグ画面
			Config_Background = TxC(CONFIG + @"Background.png");
			Config_Cursor = TxC(CONFIG + @"Cursor.png");
			Config_ItemBox = TxC(CONFIG + @"ItemBox.png");
			Config_Arrow = TxC(CONFIG + @"Arrow.png");
			Config_KeyAssign = TxC(CONFIG + @"KeyAssign.png");
			Config_Font = TxC(CONFIG + @"Font.png");
			Config_Font_Bold = TxC(CONFIG + @"Font_Bold.png");
			Config_Enum_Song = TxC(CONFIG + @"Enum_Song.png");
			#endregion

			#region 3_選曲画面
			SongSelect_Background = TxC(SONGSELECT + @"Background.png");
			SongSelect_Header = TxC(SONGSELECT + @"Header.png");
			SongSelect_Footer = TxC(SONGSELECT + @"Footer.png");
			SongSelect_Difficulty = TxC(SONGSELECT + @"Difficulty.png");
			SongSelect_Auto = TxC(SONGSELECT + @"Auto.png");
			SongSelect_Level = TxC(SONGSELECT + @"Level.png");
			SongSelect_Branch = TxC(SONGSELECT + @"Branch.png");
			SongSelect_Branch_Text = TxC(SONGSELECT + @"Branch_Text.png");
			SongSelect_Branch_Text_NEW = TxC(SONGSELECT + @"Branch_Text_NEW.png");
			SongSelect_Bar_Center = TxC(SONGSELECT + @"Bar_Center.png");
			SongSelect_Frame_Score = TxC(SONGSELECT + @"Frame_Score.png");
			SongSelect_Frame_Box = TxC(SONGSELECT + @"Frame_Box.png");
			SongSelect_Frame_BackBox = TxC(SONGSELECT + @"Frame_BackBox.png");
			SongSelect_Frame_Random = TxC(SONGSELECT + @"Frame_Random.png");
			//SongSelect_Score_Select = TxC(SONGSELECT + @"Score_Select.png");
			//SongSelect_Frame_Dani = TxC(SONGSELECT + @"Frame_Dani.png");
			SongSelect_Cursor_Left = TxC(SONGSELECT + @"Cursor_Left.png");
			SongSelect_Cursor_Right = TxC(SONGSELECT + @"Cursor_Right.png");
			SongSelect_Bar_BackBox = TxC(SONGSELECT + @"Bar_BackBox.png");

			for (int i = 0; i < SongSelect_Lyric_Text.Length; i++)
			{
				SongSelect_Lyric_Text[i] = TxC(SONGSELECT + @"Lyric_Text_" + i.ToString() + ".png");
			}
			for (int i = 0; i < SongSelect_Bar_Genre.Length; i++)
			{
				SongSelect_Bar_Genre[i] = TxC(SONGSELECT + @"Bar_Genre_" + i.ToString() + ".png");
			}
			for (int i = 0; i < SongSelect_Bar_Box_Genre.Length; i++)
			{
				SongSelect_Bar_Box_Genre[i] = TxC(SONGSELECT + @"Bar_Box_Genre_" + i.ToString() + ".png");
			}
			for (int i = 0; i <  SongSelect_Box_Center_Genre.Length; i++)
			{
				SongSelect_Box_Center_Genre[i] = TxC(SONGSELECT + BOX_CENTER + @"Box_Center_Genre_" + i.ToString() + ".png");
			}
			for (int i = 0; i < SongSelect_Box_Center_Header_Genre.Length; i++)
			{
				SongSelect_Box_Center_Header_Genre[i] = TxC(SONGSELECT + BOX_CENTER + @"Box_Center_Header_Genre_" + i.ToString() + ".png");
			}
			for (int i = 0; i < SongSelect_Box_Center_Text_Genre.Length; i++)
			{
				SongSelect_Box_Center_Text_Genre[i] = TxC(SONGSELECT +BOX_CENTER+ @"Box_Center_Text_Genre_" + i.ToString() + ".png");
			}
			for (int i = 0; i < SongSelect_Bar_Center_Back_Genre.Length; i++)
			{
				SongSelect_Bar_Center_Back_Genre[i] = TxC(SONGSELECT + @"Bar_Center_Background_Genre_" + i.ToString() + ".png");
			}
			for (int i = 0; i < (int)Difficulty.Total; i++)
			{
				SongSelect_ScoreWindow[i] = TxC(SONGSELECT + @"ScoreWindow_" + i.ToString() + ".png");
			}

			for (int i = 0; i < SongSelect_GenreBack.Length; i++)
			{
				SongSelect_GenreBack[i] = TxC(SONGSELECT + @"GenreBackground_" + i.ToString() + ".png");
			}
			for (int i = 0; i < SongSelect_Counter_Back.Length; i++)
			{
				SongSelect_Counter_Back[i] = TxC(SONGSELECT + @"Counter_Background_" + i.ToString() + ".png");
			}
			for (int i = 0; i < SongSelect_Counter_Num.Length; i++)
			{
				SongSelect_Counter_Num[i] = TxC(SONGSELECT + @"Counter_Number_" + i.ToString() + ".png");
			}
			SongSelect_ScoreWindow_Text = TxC(SONGSELECT + @"ScoreWindow_Text.png");


			#region[3.5_難易度選択]
			Difficulty_Dan_Box = TxC(SONGSELECT + DIFFICULITY + @"Dan_Box.png");
			Difficulty_Dan_Box_Selecting = TxC(SONGSELECT + DIFFICULITY + @"Dan_Box_Selecting.png");
			Difficulty_Star = TxC(SONGSELECT + DIFFICULITY + @"Difficulty_Star.png");
			Difficulty_Branch = TxC(SONGSELECT + DIFFICULITY + @"Difficulty_Branch.png");
			Difficulty_Center_Bar = TxC(SONGSELECT + DIFFICULITY + @"Difficulty_Center_Bar.png");
			Difficulty_Bar_Etc[0] = TxC(SONGSELECT + DIFFICULITY + @"Difficulty_Bar_Back.png");
			Difficulty_Bar_Etc[1] = TxC(SONGSELECT + DIFFICULITY + @"Difficulty_Bar_Option.png");
			Difficulty_Bar_Etc[2] = TxC(SONGSELECT + DIFFICULITY + @"Difficulty_Bar_Sound.png");
			ChangeSE_Box = TxC(SONGSELECT + DIFFICULITY + @"ChangeSE_Box.png");
			ChangeSE_Note = TxC(SONGSELECT + DIFFICULITY + @"ChangeSE_Note.png");
			ChangeSE_Num = TxC(SONGSELECT + DIFFICULITY + @"ChangeSE_Num.png");
			PlayOption_List = TxC(SONGSELECT + DIFFICULITY + @"PlayOption_List.png");
			PlayOption_Active = TxC(SONGSELECT + DIFFICULITY + @"PlayOption_Active.png");

			for (int i = 0; i < Difficulty_Bar.Length; i++)
			{
				Difficulty_Bar[i] = TxC(SONGSELECT + DIFFICULITY + @"Difficulty_Bar_" + i.ToString() + ".png");
			}
			for (int i = 0; i < Difficulty_Anc.Length; i++)
			{
				Difficulty_Anc[i] = TxC(SONGSELECT + DIFFICULITY + @"Difficulty_Anc_" + (i+1).ToString() + "P.png");
			}
			for (int i = 0; i < Difficulty_Anc_Same.Length; i++)
			{
				Difficulty_Anc_Same[i] = TxC(SONGSELECT + DIFFICULITY + @"Difficulty_Anc_Same_" + (i + 1).ToString() + "P.png");
			}
			for (int i = 0; i < Difficulty_Anc_Box.Length; i++)
			{
				Difficulty_Anc_Box[i] = TxC(SONGSELECT + DIFFICULITY + @"Difficulty_Anc_Box_" + (i + 1).ToString() + "P.png");
			}
			for (int i = 0; i < Difficulty_Anc_Box_Etc.Length; i++)
			{
				Difficulty_Anc_Box_Etc[i] = TxC(SONGSELECT + DIFFICULITY + @"Difficulty_Anc_Box_Etc_" + (i + 1).ToString() + "P.png");
			}
			for (int i = 0; i < Difficulty_Mark.Length; i++)
			{
				Difficulty_Mark[i] = TxC(SONGSELECT + DIFFICULITY + @"Difficulty_Mark_" + i.ToString() + ".png");
			}
			#endregion

			#endregion

			#region 4_読み込み画面
			SongLoading_Plate = TxC(SONGLOADING + @"Plate.png");
			SongLoading_FadeIn = TxC(SONGLOADING + @"FadeIn.png");
			SongLoading_FadeOut = TxC(SONGLOADING + @"FadeOut.png");
			#endregion

			#region 5_演奏画面
			#region 共通
			Notes = TxC(GAME + @"Notes.png");
			Notes_White = TxC(GAME + @"Notes_White.png");
			Judge_Frame = TxC(GAME + @"Notes.png");
			SENotes = TxC(GAME + @"SENotes.png");
			Notes_Arm = TxC(GAME + @"Notes_Arm.png");
			Judge = TxC(GAME + @"Judge.png");

			Judge_Meter = TxC(GAME + @"Judge_Meter.png");
			Bar = TxC(GAME + @"Bar.png");
			Bar_Branch = TxC(GAME + @"Bar_Branch.png");

			#endregion
			#region キャラクター
			for (int nPlayer = 0; nPlayer < 2; nPlayer++)
			{
				string PLAYER = nPlayer.ToString() + @"\";
				TJAPlayer3.Skin.Game_Chara_Ptn_Normal[nPlayer] = TJAPlayer3.t連番画像の枚数を数える(CSkin.Path(BASE + GAME + CHARA + PLAYER + @"Normal\"));
				if (TJAPlayer3.Skin.Game_Chara_Ptn_Normal[nPlayer] != 0)
				{
					Chara_Normal[nPlayer] = new CTexture[TJAPlayer3.Skin.Game_Chara_Ptn_Normal[nPlayer]];
					for (int i = 0; i < TJAPlayer3.Skin.Game_Chara_Ptn_Normal[nPlayer]; i++)
					{
						Chara_Normal[nPlayer][i] = TxC(GAME + CHARA + PLAYER + @"Normal\" + i.ToString() + ".png");
					}
				}
				TJAPlayer3.Skin.Game_Chara_Ptn_Clear[nPlayer] = TJAPlayer3.t連番画像の枚数を数える(CSkin.Path(BASE + GAME + CHARA + PLAYER + @"Clear\"));
				if (TJAPlayer3.Skin.Game_Chara_Ptn_Clear[nPlayer] != 0)
				{
					Chara_Normal_Cleared[nPlayer] = new CTexture[TJAPlayer3.Skin.Game_Chara_Ptn_Clear[nPlayer]];
					for (int i = 0; i < TJAPlayer3.Skin.Game_Chara_Ptn_Clear[nPlayer]; i++)
					{
						Chara_Normal_Cleared[nPlayer][i] = TxC(GAME + CHARA + PLAYER + @"Clear\" + i.ToString() + ".png");
					}
				}
				if (TJAPlayer3.Skin.Game_Chara_Ptn_Clear[nPlayer] != 0)
				{
					Chara_Normal_Maxed[nPlayer] = new CTexture[TJAPlayer3.Skin.Game_Chara_Ptn_Clear[nPlayer]];
					for (int i = 0; i < TJAPlayer3.Skin.Game_Chara_Ptn_Clear[nPlayer]; i++)
					{
						Chara_Normal_Maxed[nPlayer][i] = TxC(GAME + CHARA + PLAYER + @"Clear_Max\" + i.ToString() + ".png");
					}
				}
				TJAPlayer3.Skin.Game_Chara_Ptn_GoGo[nPlayer] = TJAPlayer3.t連番画像の枚数を数える(CSkin.Path(BASE + GAME + CHARA + PLAYER + @"GoGo\"));
				if (TJAPlayer3.Skin.Game_Chara_Ptn_GoGo[nPlayer] != 0)
				{
					Chara_GoGoTime[nPlayer] = new CTexture[TJAPlayer3.Skin.Game_Chara_Ptn_GoGo[nPlayer]];
					for (int i = 0; i < TJAPlayer3.Skin.Game_Chara_Ptn_GoGo[nPlayer]; i++)
					{
						Chara_GoGoTime[nPlayer][i] = TxC(GAME + CHARA + PLAYER + @"GoGo\" + i.ToString() + ".png");
					}
				}
				if (TJAPlayer3.Skin.Game_Chara_Ptn_GoGo[nPlayer] != 0)
				{
					Chara_GoGoTime_Maxed[nPlayer] = new CTexture[TJAPlayer3.Skin.Game_Chara_Ptn_GoGo[nPlayer]];
					for (int i = 0; i < TJAPlayer3.Skin.Game_Chara_Ptn_GoGo[nPlayer]; i++)
					{
						Chara_GoGoTime_Maxed[nPlayer][i] = TxC(GAME + CHARA + PLAYER + @"GoGo_Max\" + i.ToString() + ".png");
					}
				}

				TJAPlayer3.Skin.Game_Chara_Ptn_10combo[nPlayer] = TJAPlayer3.t連番画像の枚数を数える(CSkin.Path(BASE + GAME + CHARA + PLAYER + @"10combo\"));
				if (TJAPlayer3.Skin.Game_Chara_Ptn_10combo[nPlayer] != 0)
				{
					Chara_10Combo[nPlayer] = new CTexture[TJAPlayer3.Skin.Game_Chara_Ptn_10combo[nPlayer]];
					for (int i = 0; i < TJAPlayer3.Skin.Game_Chara_Ptn_10combo[nPlayer]; i++)
					{
						Chara_10Combo[nPlayer][i] = TxC(GAME + CHARA + PLAYER + @"10combo\" + i.ToString() + ".png");
					}
				}
				TJAPlayer3.Skin.Game_Chara_Ptn_10combo_Max[nPlayer] = TJAPlayer3.t連番画像の枚数を数える(CSkin.Path(BASE + GAME + CHARA + PLAYER + @"10combo_Max\"));
				if (TJAPlayer3.Skin.Game_Chara_Ptn_10combo_Max[nPlayer] != 0)
				{
					Chara_10Combo_Maxed[nPlayer] = new CTexture[TJAPlayer3.Skin.Game_Chara_Ptn_10combo_Max[nPlayer]];
					for (int i = 0; i < TJAPlayer3.Skin.Game_Chara_Ptn_10combo_Max[nPlayer]; i++)
					{
						Chara_10Combo_Maxed[nPlayer][i] = TxC(GAME + CHARA + PLAYER + @"10combo_Max\" + i.ToString() + ".png");
					}
				}

				TJAPlayer3.Skin.Game_Chara_Ptn_GoGoStart[nPlayer] = TJAPlayer3.t連番画像の枚数を数える(CSkin.Path(BASE + GAME + CHARA + PLAYER + @"GoGoStart\"));
				if (TJAPlayer3.Skin.Game_Chara_Ptn_GoGoStart[nPlayer] != 0)
				{
					Chara_GoGoStart[nPlayer] = new CTexture[TJAPlayer3.Skin.Game_Chara_Ptn_GoGoStart[nPlayer]];
					for (int i = 0; i < TJAPlayer3.Skin.Game_Chara_Ptn_GoGoStart[nPlayer]; i++)
					{
						Chara_GoGoStart[nPlayer][i] = TxC(GAME + CHARA + PLAYER + @"GoGoStart\" + i.ToString() + ".png");
					}
				}
				TJAPlayer3.Skin.Game_Chara_Ptn_GoGoStart_Max[nPlayer] = TJAPlayer3.t連番画像の枚数を数える(CSkin.Path(BASE + GAME + CHARA + PLAYER + @"GoGoStart_Max\"));
				if (TJAPlayer3.Skin.Game_Chara_Ptn_GoGoStart_Max[nPlayer] != 0)
				{
					Chara_GoGoStart_Maxed[nPlayer] = new CTexture[TJAPlayer3.Skin.Game_Chara_Ptn_GoGoStart_Max[nPlayer]];
					for (int i = 0; i < TJAPlayer3.Skin.Game_Chara_Ptn_GoGoStart_Max[nPlayer]; i++)
					{
						Chara_GoGoStart_Maxed[nPlayer][i] = TxC(GAME + CHARA + PLAYER + @"GoGoStart_Max\" + i.ToString() + ".png");
					}
				}
				TJAPlayer3.Skin.Game_Chara_Ptn_ClearIn[nPlayer] = TJAPlayer3.t連番画像の枚数を数える(CSkin.Path(BASE + GAME + CHARA + PLAYER + @"ClearIn\"));
				if (TJAPlayer3.Skin.Game_Chara_Ptn_ClearIn[nPlayer] != 0)
				{
					Chara_Become_Cleared[nPlayer] = new CTexture[TJAPlayer3.Skin.Game_Chara_Ptn_ClearIn[nPlayer]];
					for (int i = 0; i < TJAPlayer3.Skin.Game_Chara_Ptn_ClearIn[nPlayer]; i++)
					{
						Chara_Become_Cleared[nPlayer][i] = TxC(GAME + CHARA + PLAYER + @"ClearIn\" + i.ToString() + ".png");
					}
				}
				TJAPlayer3.Skin.Game_Chara_Ptn_SoulIn[nPlayer] = TJAPlayer3.t連番画像の枚数を数える(CSkin.Path(BASE + GAME + CHARA + PLAYER + @"SoulIn\"));
				if (TJAPlayer3.Skin.Game_Chara_Ptn_SoulIn[nPlayer] != 0)
				{
					Chara_Become_Maxed[nPlayer] = new CTexture[TJAPlayer3.Skin.Game_Chara_Ptn_SoulIn[nPlayer]];
					for (int i = 0; i < TJAPlayer3.Skin.Game_Chara_Ptn_SoulIn[nPlayer]; i++)
					{
						Chara_Become_Maxed[nPlayer][i] = TxC(GAME + CHARA + PLAYER + @"SoulIn\" + i.ToString() + ".png");
					}
				}
				TJAPlayer3.Skin.Game_Chara_Ptn_Balloon_Breaking[nPlayer] = TJAPlayer3.t連番画像の枚数を数える(CSkin.Path(BASE + GAME + CHARA + PLAYER + @"Balloon_Breaking\"));
				if (TJAPlayer3.Skin.Game_Chara_Ptn_Balloon_Breaking[nPlayer] != 0)
				{
					Chara_Balloon_Breaking[nPlayer] = new CTexture[TJAPlayer3.Skin.Game_Chara_Ptn_Balloon_Breaking[nPlayer]];
					for (int i = 0; i < TJAPlayer3.Skin.Game_Chara_Ptn_Balloon_Breaking[nPlayer]; i++)
					{
						Chara_Balloon_Breaking[nPlayer][i] = TxC(GAME + CHARA + PLAYER + @"Balloon_Breaking\" + i.ToString() + ".png");
					}
				}
				TJAPlayer3.Skin.Game_Chara_Ptn_Balloon_Broke[nPlayer] = TJAPlayer3.t連番画像の枚数を数える(CSkin.Path(BASE + GAME + CHARA + PLAYER + @"Balloon_Broke\"));
				if (TJAPlayer3.Skin.Game_Chara_Ptn_Balloon_Broke[nPlayer] != 0)
				{
					Chara_Balloon_Broke[nPlayer] = new CTexture[TJAPlayer3.Skin.Game_Chara_Ptn_Balloon_Broke[nPlayer]];
					for (int i = 0; i < TJAPlayer3.Skin.Game_Chara_Ptn_Balloon_Broke[nPlayer]; i++)
					{
						Chara_Balloon_Broke[nPlayer][i] = TxC(GAME + CHARA + PLAYER + @"Balloon_Broke\" + i.ToString() + ".png");
					}
				}
				TJAPlayer3.Skin.Game_Chara_Ptn_Balloon_Miss[nPlayer] = TJAPlayer3.t連番画像の枚数を数える(CSkin.Path(BASE + GAME + CHARA + PLAYER + @"Balloon_Miss\"));
				if (TJAPlayer3.Skin.Game_Chara_Ptn_Balloon_Miss[nPlayer] != 0)
				{
					Chara_Balloon_Miss[nPlayer] = new CTexture[TJAPlayer3.Skin.Game_Chara_Ptn_Balloon_Miss[nPlayer]];
					for (int i = 0; i < TJAPlayer3.Skin.Game_Chara_Ptn_Balloon_Miss[nPlayer]; i++)
					{
						Chara_Balloon_Miss[nPlayer][i] = TxC(GAME + CHARA + PLAYER + @"Balloon_Miss\" + i.ToString() + ".png");
					}
				}
			}
			#endregion
			#region 踊り子
			TJAPlayer3.Skin.Game_Dancer_Ptn = TJAPlayer3.t連番画像の枚数を数える(CSkin.Path(BASE + GAME + DANCER + @"1\"));
			if (TJAPlayer3.Skin.Game_Dancer_Ptn != 0)
			{
				Dancer = new CTexture[5][];
				for (int i = 0; i < Dancer.Length; i++)
				{
					Dancer[i] = new CTexture[TJAPlayer3.Skin.Game_Dancer_Ptn];
					for (int p = 0; p < TJAPlayer3.Skin.Game_Dancer_Ptn; p++)
					{
						Dancer[i][p] = TxC(GAME + DANCER + (i + 1) + @"\" + p.ToString() + ".png");
					}
				}
			}
			#endregion
			#region モブ
			TJAPlayer3.Skin.Game_Mob_Ptn = TJAPlayer3.t連番画像の枚数を数える(CSkin.Path(BASE + GAME + MOB));
			Mob = new CTexture[TJAPlayer3.Skin.Game_Mob_Ptn];
			for (int i = 0; i < TJAPlayer3.Skin.Game_Mob_Ptn; i++)
			{
				Mob[i] = TxC(GAME + MOB + i.ToString() + ".png");
			}
			#endregion
			#region フッター
			Mob_Footer = TxC(GAME + FOOTER + @"0.png");
			#endregion
			#region 背景
			Background = TxC(GAME + Background + @"0\" + @"Background.png");
			Background_Up = new CTexture[2];
			Background_Up[0] = TxC(GAME + BACKGROUND + @"0\" + @"1P_Up.png");
			Background_Up[1] = TxC(GAME + BACKGROUND + @"0\" + @"2P_Up.png");
			Background_Up_Clear = new CTexture[2];
			Background_Up_Clear[0] = TxC(GAME + BACKGROUND + @"0\" + @"1P_Up_Clear.png");
			Background_Up_Clear[1] = TxC(GAME + BACKGROUND + @"0\" + @"2P_Up_Clear.png");
			Background_Up_YMove = new CTexture[2];
			Background_Up_YMove[0] = TxC(GAME + BACKGROUND + @"0\" + @"1P_Up_YMove.png");
			Background_Up_YMove[1] = TxC(GAME + BACKGROUND + @"0\" + @"2P_Up_YMove.png");
			Background_Up_YMove_Clear = new CTexture[2];
			Background_Up_YMove_Clear[0] = TxC(GAME + BACKGROUND + @"0\" + @"1P_Up_YMove_Clear.png");
			Background_Up_YMove_Clear[1] = TxC(GAME + BACKGROUND + @"0\" + @"2P_Up_YMove_Clear.png");
			Background_Up_Sakura = new CTexture[2];
			Background_Up_Sakura[0] = TxC(GAME + BACKGROUND + @"0\" + @"1P_Up_Sakura.png");
			Background_Up_Sakura[1] = TxC(GAME + BACKGROUND + @"0\" + @"2P_Up_Sakura.png");
			Background_Up_Sakura_Clear = new CTexture[2];
			Background_Up_Sakura_Clear[0] = TxC(GAME + BACKGROUND + @"0\" + @"1P_Up_Sakura_Clear.png");
			Background_Up_Sakura_Clear[1] = TxC(GAME + BACKGROUND + @"0\" + @"2P_Up_Sakura_Clear.png");
			Background_Down = TxC(GAME + BACKGROUND + @"0\" + @"Down.png");
			Background_Down_Clear = TxC(GAME + BACKGROUND + @"0\" + @"Down_Clear.png");
			Background_Down_Scroll = TxC(GAME + BACKGROUND + @"0\" + @"Down_Scroll.png");

			#endregion
			#region 太鼓
			Taiko_Background = new CTexture[2];
			Taiko_Background[0] = TxC(GAME + TAIKO + @"1P_Background.png");
			Taiko_Background[1] = TxC(GAME + TAIKO + @"2P_Background.png");
			Taiko_Frame = new CTexture[2];
			Taiko_Frame[0] = TxC(GAME + TAIKO + @"1P_Frame.png");
			Taiko_Frame[1] = TxC(GAME + TAIKO + @"2P_Frame.png");
			Taiko_PlayerNumber = new CTexture[2];
			Taiko_PlayerNumber[0] = TxC(GAME + TAIKO + @"1P_PlayerNumber.png");
			Taiko_PlayerNumber[1] = TxC(GAME + TAIKO + @"2P_PlayerNumber.png");
			Taiko_NamePlate = new CTexture[2];
			Taiko_NamePlate[0] = TxC(GAME + TAIKO + @"1P_NamePlate.png");
			Taiko_NamePlate[1] = TxC(GAME + TAIKO + @"2P_NamePlate.png");
			Taiko_Base = TxC(GAME + TAIKO + @"Base.png");
			Taiko_Don_Left = TxC(GAME + TAIKO + @"Don.png");
			Taiko_Don_Right = TxC(GAME + TAIKO + @"Don.png");
			Taiko_Ka_Left = TxC(GAME + TAIKO + @"Ka.png");
			Taiko_Ka_Right = TxC(GAME + TAIKO + @"Ka.png");
			Taiko_LevelUp = TxC(GAME + TAIKO + @"LevelUp.png");
			Taiko_LevelDown = TxC(GAME + TAIKO + @"LevelDown.png");
			Couse_Symbol = new CTexture[(int)Difficulty.Total + 1]; // +1は真打ちモードの分
			string[] Couse_Symbols = new string[(int)Difficulty.Total + 1] { "Easy", "Normal", "Hard", "Oni", "Edit", "Tower", "Dan", "Shin" };
			for (int i = 0; i < (int)Difficulty.Total + 1; i++)
			{
				Couse_Symbol[i] = TxC(GAME + COURSESYMBOL + Couse_Symbols[i] + ".png");
			}
			Taiko_Score = new CTexture[3];
			Taiko_Score[0] = TxC(GAME + TAIKO + @"Score.png");
			Taiko_Score[1] = TxC(GAME + TAIKO + @"Score_1P.png");
			Taiko_Score[2] = TxC(GAME + TAIKO + @"Score_2P.png");
			Taiko_Combo = new CTexture[2];
			Taiko_Combo[0] = TxC(GAME + TAIKO + @"Combo.png");
			Taiko_Combo[1] = TxC(GAME + TAIKO + @"Combo_Big.png");
			Taiko_Combo_Effect = TxC(GAME + TAIKO + @"Combo_Effect.png");
			Taiko_Combo_Text = TxC(GAME + TAIKO + @"Combo_Text.png");
			#endregion
			#region ゲージ
			Gauge = new CTexture[2];
			Gauge[0] = TxC(GAME + GAUGE + @"1P.png");
			Gauge[1] = TxC(GAME + GAUGE + @"2P.png");
			Gauge_Base = new CTexture[2];
			Gauge_Base[0] = TxC(GAME + GAUGE + @"1P_Base.png");
			Gauge_Base[1] = TxC(GAME + GAUGE + @"2P_Base.png");
			Gauge_Line = new CTexture[2];
			Gauge_Line[0] = TxC(GAME + GAUGE + @"1P_Line.png");
			Gauge_Line[1] = TxC(GAME + GAUGE + @"2P_Line.png");
			TJAPlayer3.Skin.Game_Gauge_Rainbow_Ptn = TJAPlayer3.t連番画像の枚数を数える(CSkin.Path(BASE + GAME + GAUGE + @"Rainbow\"));
			if (TJAPlayer3.Skin.Game_Gauge_Rainbow_Ptn != 0)
			{
				Gauge_Rainbow = new CTexture[TJAPlayer3.Skin.Game_Gauge_Rainbow_Ptn];
				for (int i = 0; i < TJAPlayer3.Skin.Game_Gauge_Rainbow_Ptn; i++)
				{
					Gauge_Rainbow[i] = TxC(GAME + GAUGE + @"Rainbow\" + i.ToString() + ".png");
				}
			}
			Gauge_Soul = TxC(GAME + GAUGE + @"Soul.png");
			Gauge_Soul_Fire = TxC(GAME + GAUGE + @"Fire.png");
			Gauge_Soul_Explosion = new CTexture[2];
			Gauge_Soul_Explosion[0] = TxC(GAME + GAUGE + @"1P_Explosion.png");
			Gauge_Soul_Explosion[1] = TxC(GAME + GAUGE + @"2P_Explosion.png");

            #region[Gauge_DanC]
			Gauge_Danc = TxC(GAME + GAUGE + @"DanC\" + @"1P.png");
			Gauge_Base_Danc = TxC(GAME + GAUGE + @"DanC\" + @"1P_Base.png");
			Gauge_Line_Danc = TxC(GAME + GAUGE + @"DanC\" + @"1P_Line.png");
			TJAPlayer3.Skin.Game_Gauge_Rainbow_Danc_Ptn = TJAPlayer3.t連番画像の枚数を数える(CSkin.Path(BASE + GAME + GAUGE + @"DanC\" + @"Rainbow\"));
			if (TJAPlayer3.Skin.Game_Gauge_Rainbow_Danc_Ptn != 0)
			{
				Gauge_Rainbow_Danc = new CTexture[TJAPlayer3.Skin.Game_Gauge_Rainbow_Danc_Ptn];
				for (int i = 0; i < TJAPlayer3.Skin.Game_Gauge_Rainbow_Danc_Ptn; i++)
				{
					Gauge_Rainbow_Danc[i] = TxC(GAME + GAUGE + @"DanC\" + @"Rainbow\" + i.ToString() + ".png");
				}
			}
            #endregion

            #endregion
            #region 吹き出し
            Balloon_Combo = new CTexture[2];
			Balloon_Combo[0] = TxC(GAME + BALLOON + @"Combo_1P.png");
			Balloon_Combo[1] = TxC(GAME + BALLOON + @"Combo_2P.png");
			Balloon_Roll = TxC(GAME + BALLOON + @"Roll.png");
			Balloon_Balloon = TxC(GAME + BALLOON + @"Balloon.png");
			Balloon_Number_Roll = TxC(GAME + BALLOON + @"Number_Roll.png");
			Balloon_Number_Combo = TxC(GAME + BALLOON + @"Number_Combo.png");

			Balloon_Breaking = new CTexture[6];
			for (int i = 0; i < 6; i++)
			{
				Balloon_Breaking[i] = TxC(GAME + BALLOON + @"Breaking_" + i.ToString() + ".png");
			}
			#endregion
			#region エフェクト
			Effects_Hit_Explosion = TxC(GAME + EFFECTS + @"Hit\Explosion.png");
			if (Effects_Hit_Explosion != null) Effects_Hit_Explosion.b加算合成 = TJAPlayer3.Skin.Game_Effect_HitExplosion_AddBlend;
			Effects_Hit_Explosion_Big = TxC(GAME + EFFECTS + @"Hit\Explosion_Big.png");
			if (Effects_Hit_Explosion_Big != null) Effects_Hit_Explosion_Big.b加算合成 = TJAPlayer3.Skin.Game_Effect_HitExplosionBig_AddBlend;
			Effects_Hit_FireWorks = TxC(GAME + EFFECTS + @"Hit\FireWorks.png");
			if (Effects_Hit_FireWorks != null) Effects_Hit_FireWorks.b加算合成 = TJAPlayer3.Skin.Game_Effect_FireWorks_AddBlend;


			Effects_Fire = TxC(GAME + EFFECTS + @"Fire.png");
			if (Effects_Fire != null) Effects_Fire.b加算合成 = TJAPlayer3.Skin.Game_Effect_Fire_AddBlend;

			Effects_Rainbow = TxC(GAME + EFFECTS + @"Rainbow.png");

			Effects_GoGoSplash = TxC(GAME + EFFECTS + @"GoGoSplash.png");
			if (Effects_GoGoSplash != null) Effects_GoGoSplash.b加算合成 = TJAPlayer3.Skin.Game_Effect_GoGoSplash_AddBlend;
			Effects_Hit_Great = new CTexture[15];
			Effects_Hit_Great_Big = new CTexture[15];
			Effects_Hit_Good = new CTexture[15];
			Effects_Hit_Good_Big = new CTexture[15];
			for (int i = 0; i < 15; i++)
			{
				Effects_Hit_Great[i] = TxC(GAME + EFFECTS + @"Hit\" + @"Great\" + i.ToString() + ".png");
				Effects_Hit_Great_Big[i] = TxC(GAME + EFFECTS + @"Hit\" + @"Great_Big\" + i.ToString() + ".png");
				Effects_Hit_Good[i] = TxC(GAME + EFFECTS + @"Hit\" + @"Good\" + i.ToString() + ".png");
				Effects_Hit_Good_Big[i] = TxC(GAME + EFFECTS + @"Hit\" + @"Good_Big\" + i.ToString() + ".png");
			}
			TJAPlayer3.Skin.Game_Effect_Roll_Ptn = TJAPlayer3.t連番画像の枚数を数える(CSkin.Path(BASE + GAME + EFFECTS + @"Roll\"));
			Effects_Roll = new CTexture[TJAPlayer3.Skin.Game_Effect_Roll_Ptn];
			for (int i = 0; i < TJAPlayer3.Skin.Game_Effect_Roll_Ptn; i++)
			{
				Effects_Roll[i] = TxC(GAME + EFFECTS + @"Roll\" + i.ToString() + ".png");
			}
			#endregion
			#region レーン
			Lane_Base = new CTexture[3];
			Lane_Text = new CTexture[3];
			string[] Lanes = new string[3] { "Normal", "Expert", "Master" };
			for (int i = 0; i < 3; i++)
			{
				Lane_Base[i] = TxC(GAME + LANE + "Base_" + Lanes[i] + ".png");
				Lane_Text[i] = TxC(GAME + LANE + "Text_" + Lanes[i] + ".png");
			}
			Lane_Red = TxC(GAME + LANE + @"Red.png");
			Lane_Blue = TxC(GAME + LANE + @"Blue.png");
			Lane_Yellow = TxC(GAME + LANE + @"Yellow.png");
			Lane_Background_Main = TxC(GAME + LANE + @"Background_Main.png");
			Lane_Background_Sub = TxC(GAME + LANE + @"Background_Sub.png");
			Lane_Background_GoGo = TxC(GAME + LANE + @"Background_GoGo.png");

			#endregion
			#region 終了演出
			End_Failed_L = new CTexture[2];
			End_Failed_R = new CTexture[2];
			for (int i = 0; i < 2; i++)
			{
				End_Failed_L[i] = TxC(GAME + END + @"Failed_L_" + i.ToString() + ".png");
				End_Failed_R[i] = TxC(GAME + END + @"Failed_R_" + i.ToString() + ".png");
			}
			End_Clear_L = new CTexture[5];
			End_Clear_R = new CTexture[5];
			for (int i = 0; i < 5; i++)
			{
				End_Clear_L[i] = TxC(GAME + END + @"Clear_L_" + i.ToString() + ".png");
				End_Clear_R[i] = TxC(GAME + END + @"Clear_R_" + i.ToString() + ".png");
			}
			End_Fan = new CTexture[4];
			for (int i = 0; i < 4; i++)
			{
				End_Fan[i] = TxC(GAME + END + @"Fan_" + i.ToString() + ".png");
			}
			End_Failed_Impact = TxC(GAME + END + @"Failed_Impact.png");
			End_Failed_Text = TxC(GAME + END + @"Failed_Text.png");
			End_Clear_Text = TxC(GAME + END + @"Clear_Text.png");
			End_Clear_Text_Effect = TxC(GAME + END + @"Clear_Text_Effect.png");
			End_FullCombo_Text = TxC(GAME + END + @"FullCombo_Text.png");
			End_FullCombo_Text_Effect = TxC(GAME + END + @"FullCombo_Text_Effect.png");
			End_DonderFullCombo_Lane = TxC(GAME + END + @"DonderFullCombo_Lane.png");
			End_DonderFullCombo_L = TxC(GAME + END + @"DonderFullCombo_L.png");
			End_DonderFullCombo_R = TxC(GAME + END + @"DonderFullCombo_R.png");
			End_DonderFullCombo_Text = TxC(GAME + END + @"DonderFullCombo_Text.png");
			End_DonderFullCombo_Text_Effect = TxC(GAME + END + @"DonderFullCombo_Text_Effect.png");
			if (End_Clear_Text_Effect != null) End_Clear_Text_Effect.b加算合成 = true;
			if (End_FullCombo_Text_Effect != null) End_FullCombo_Text_Effect.b加算合成 = true;
			if (End_DonderFullCombo_Text_Effect != null) End_DonderFullCombo_Text_Effect.b加算合成 = true;
			#endregion
			#region ゲームモード
			GameMode_Timer_Tick = TxC(GAME + GAMEMODE + @"Timer_Tick.png");
			GameMode_Timer_Frame = TxC(GAME + GAMEMODE + @"Timer_Frame.png");
			#endregion
			#region ステージ失敗
			Failed_Game = TxC(GAME + FAILED + @"Game.png");
			Failed_Stage = TxC(GAME + FAILED + @"Stage.png");
			#endregion
			#region ランナー
			Runner = TxC(GAME + RUNNER + @"0.png");
			#endregion
			#region DanC
			DanC_Background = TxC(GAME + DANC + @"Background.png");
			DanC_Gauge = new CTexture[4];
			var type = new string[] { "Normal", "Reach", "Clear", "Flush" };
			for (int i = 0; i < 4; i++)
			{
				DanC_Gauge[i] = TxC(GAME + DANC + @"Gauge_" + type[i] + ".png");
			}
			DanC_Base = TxC(GAME + DANC + @"Base.png");
			DanC_Failed = TxC(GAME + DANC + @"Failed.png");
			DanC_Number = TxC(GAME + DANC + @"Number.png");
			DanC_ExamType = TxC(GAME + DANC + @"ExamType.png");
			DanC_ExamRange = TxC(GAME + DANC + @"ExamRange.png");
			DanC_ExamUnit = TxC(GAME + DANC + @"ExamUnit.png");
			DanC_Screen = TxC(GAME + DANC + @"Screen.png");
			#endregion
			#region PuichiChara
			PuchiChara = new CTexture[2];
			for (int i = 0; i < 2; i++)
			{
				PuchiChara[i] = TxC(GAME + PUCHICHARA + i.ToString() + @".png");
			}
			#endregion
			#region Training
			Tokkun_DownBG = TxC(GAME + TRAINING + @"Down.png");
			Tokkun_BigTaiko = TxC(GAME + TRAINING + @"BigTaiko.png");
			Tokkun_ProgressBar = TxC(GAME + TRAINING + @"ProgressBar_Red.png");
			Tokkun_ProgressBarWhite = TxC(GAME + TRAINING + @"ProgressBar_White.png");
			Tokkun_GoGoPoint = TxC(GAME + TRAINING + @"GoGoPoint.png");
			Tokkun_JumpPoint = TxC(GAME + TRAINING + @"JumpPoint.png");
			Tokkun_Background_Up = TxC(GAME + TRAINING + @"Background_Up.png");
			Tokkun_BigNumber = TxC(GAME + TRAINING + @"BigNumber.png");
			Tokkun_SmallNumber = TxC(GAME + TRAINING + @"SmallNumber.png");
			Tokkun_Speed_Measure = TxC(GAME + TRAINING + @"Speed_Measure.png");
			#endregion
			#endregion

			#region 6_結果発表
			Result_Background = TxC(RESULT + @"Background.png");
			Result_FadeIn = TxC(RESULT + @"FadeIn.png");
			Result_Gauge = TxC(RESULT + @"Gauge.png");
			Result_Gauge_Base = TxC(RESULT + @"Gauge_Base.png");
			Result_Judge = TxC(RESULT + @"Judge.png");
			Result_Header = TxC(RESULT + @"Header.png");
			Result_Number = TxC(RESULT + @"Number.png");
			Result_Panel = TxC(RESULT + @"Panel.png");
			Result_Score_Text = TxC(RESULT + @"Score_Text.png");
			Result_Score_Number = TxC(RESULT + @"Score_Number.png");
			Result_Dan = TxC(RESULT + @"Dan.png");
			#endregion

			#region 7_終了画面
			Exit_Background = TxC(EXIT + @"Background.png");
			Exit_Curtain = TxC(EXIT + @"Curtain.png");
			Exit_Text = TxC(EXIT + @"Text.png");
			#endregion

		}

		public void DisposeTexture()
		{
			#region 共通
			TJAPlayer3.t安全にDisposeする(ref Tile_Black);
			TJAPlayer3.t安全にDisposeする(ref Menu_Title);
			TJAPlayer3.t安全にDisposeする(ref Menu_Highlight);
			TJAPlayer3.t安全にDisposeする(ref Enum_Song);
			TJAPlayer3.t安全にDisposeする(ref Scanning_Loudness);
			TJAPlayer3.t安全にDisposeする(ref Overlay);
			TJAPlayer3.t安全にDisposeする(ref Overlay_Offline);
			TJAPlayer3.t安全にDisposeする(ref Overlay_Online);
			TJAPlayer3.t安全にDisposeする(ref Crown_t);
			TJAPlayer3.t安全にDisposeする(ref DanC_Crown_t);
			TJAPlayer3.t安全にDisposeする(ref Difficulty_Icons);
			TJAPlayer3.t安全にDisposeする(ref NamePlate);

			#endregion

			#region 1_タイトル画面
			TJAPlayer3.t安全にDisposeする(ref Title_Background);
			TJAPlayer3.t安全にDisposeする(ref Title_Menu);
			TJAPlayer3.t安全にDisposeする(ref Title_AcBar);
			TJAPlayer3.t安全にDisposeする(ref Title_InBar);
			TJAPlayer3.t安全にDisposeする(ref Title_Txt);
			#endregion

			#region 2_コンフィグ画面
			TJAPlayer3.t安全にDisposeする(ref Config_Background);
			TJAPlayer3.t安全にDisposeする(ref Config_Cursor);
			TJAPlayer3.t安全にDisposeする(ref Config_ItemBox);
			TJAPlayer3.t安全にDisposeする(ref Config_Arrow);
			TJAPlayer3.t安全にDisposeする(ref Config_KeyAssign);
			TJAPlayer3.t安全にDisposeする(ref Config_Font);
			TJAPlayer3.t安全にDisposeする(ref Config_Font_Bold);
			TJAPlayer3.t安全にDisposeする(ref Config_Enum_Song);
			#endregion

			#region 3_選曲画面
			TJAPlayer3.t安全にDisposeする(ref SongSelect_Background);
			TJAPlayer3.t安全にDisposeする(ref SongSelect_Header);
			TJAPlayer3.t安全にDisposeする(ref SongSelect_Footer);
			TJAPlayer3.t安全にDisposeする(ref SongSelect_Difficulty);
			TJAPlayer3.t安全にDisposeする(ref SongSelect_Auto);
			TJAPlayer3.t安全にDisposeする(ref SongSelect_Level);
			TJAPlayer3.t安全にDisposeする(ref SongSelect_Branch);
			TJAPlayer3.t安全にDisposeする(ref SongSelect_Branch_Text);
			TJAPlayer3.t安全にDisposeする(ref SongSelect_Branch_Text_NEW);
			TJAPlayer3.t安全にDisposeする(ref SongSelect_Bar_Center);
			TJAPlayer3.t安全にDisposeする(ref SongSelect_Frame_Score);
			TJAPlayer3.t安全にDisposeする(ref SongSelect_Frame_Box);
			TJAPlayer3.t安全にDisposeする(ref SongSelect_Frame_BackBox);
			TJAPlayer3.t安全にDisposeする(ref SongSelect_Frame_Random);
			//TJAPlayer3.t安全にDisposeする(ref SongSelect_Score_Select);
			TJAPlayer3.t安全にDisposeする(ref SongSelect_Cursor_Left);
			TJAPlayer3.t安全にDisposeする(ref SongSelect_Cursor_Right);
			TJAPlayer3.t安全にDisposeする(ref SongSelect_Bar_BackBox);
			TJAPlayer3.t安全にDisposeする(ref SongSelect_Lyric_Text);
			TJAPlayer3.t安全にDisposeする(ref SongSelect_Bar_Genre);
			TJAPlayer3.t安全にDisposeする(ref SongSelect_Bar_Box_Genre);
			TJAPlayer3.t安全にDisposeする(ref SongSelect_Box_Center_Genre);
			TJAPlayer3.t安全にDisposeする(ref SongSelect_Box_Center_Header_Genre);
			TJAPlayer3.t安全にDisposeする(ref SongSelect_Box_Center_Text_Genre);
			TJAPlayer3.t安全にDisposeする(ref SongSelect_Bar_Center_Back_Genre);
			TJAPlayer3.t安全にDisposeする(ref SongSelect_ScoreWindow);
			TJAPlayer3.t安全にDisposeする(ref SongSelect_GenreBack);
			TJAPlayer3.t安全にDisposeする(ref SongSelect_Counter_Back);
			TJAPlayer3.t安全にDisposeする(ref SongSelect_Counter_Num);
			TJAPlayer3.t安全にDisposeする(ref SongSelect_ScoreWindow_Text);

			#region[3.5難易度選択]
			TJAPlayer3.t安全にDisposeする(ref Difficulty_Dan_Box);
			TJAPlayer3.t安全にDisposeする(ref Difficulty_Dan_Box_Selecting);
			TJAPlayer3.t安全にDisposeする(ref Difficulty_Star);
			TJAPlayer3.t安全にDisposeする(ref Difficulty_Branch);
			TJAPlayer3.t安全にDisposeする(ref Difficulty_Center_Bar);
			TJAPlayer3.t安全にDisposeする(ref ChangeSE_Box);
			TJAPlayer3.t安全にDisposeする(ref ChangeSE_Note);
			TJAPlayer3.t安全にDisposeする(ref ChangeSE_Num);
			TJAPlayer3.t安全にDisposeする(ref PlayOption_List);
			TJAPlayer3.t安全にDisposeする(ref PlayOption_Active);
			TJAPlayer3.t安全にDisposeする(ref Difficulty_Anc);
			TJAPlayer3.t安全にDisposeする(ref Difficulty_Anc_Same);
			TJAPlayer3.t安全にDisposeする(ref Difficulty_Anc_Box);
			TJAPlayer3.t安全にDisposeする(ref Difficulty_Anc_Box_Etc);
			TJAPlayer3.t安全にDisposeする(ref Difficulty_Bar);
			TJAPlayer3.t安全にDisposeする(ref Difficulty_Bar_Etc);
			TJAPlayer3.t安全にDisposeする(ref Difficulty_Mark);
			
			#endregion

			#endregion

			#region 4_読み込み画面
			TJAPlayer3.t安全にDisposeする(ref SongLoading_Plate);
			TJAPlayer3.t安全にDisposeする(ref SongLoading_FadeIn);
			TJAPlayer3.t安全にDisposeする(ref SongLoading_FadeOut);
			#endregion

			#region 5_演奏画面
			#region 共通
			TJAPlayer3.t安全にDisposeする(ref Notes);
			TJAPlayer3.t安全にDisposeする(ref Notes_White);
			TJAPlayer3.t安全にDisposeする(ref Judge_Frame);
			TJAPlayer3.t安全にDisposeする(ref SENotes);
			TJAPlayer3.t安全にDisposeする(ref Notes_Arm);
			TJAPlayer3.t安全にDisposeする(ref Judge);

			TJAPlayer3.t安全にDisposeする(ref Judge_Meter);
			TJAPlayer3.t安全にDisposeする(ref Bar);
			TJAPlayer3.t安全にDisposeする(ref Bar_Branch);

			#endregion
			#region キャラクター
			for (int nPlayer = 0; nPlayer < 2; nPlayer++)
			{
				TJAPlayer3.t安全にDisposeする(ref Chara_Normal[nPlayer]);
				TJAPlayer3.t安全にDisposeする(ref Chara_Normal_Cleared[nPlayer]);
				TJAPlayer3.t安全にDisposeする(ref Chara_Normal_Maxed[nPlayer]);
				TJAPlayer3.t安全にDisposeする(ref Chara_GoGoTime[nPlayer]);
				TJAPlayer3.t安全にDisposeする(ref Chara_GoGoTime_Maxed[nPlayer]);
				TJAPlayer3.t安全にDisposeする(ref Chara_10Combo[nPlayer]);
				TJAPlayer3.t安全にDisposeする(ref Chara_10Combo_Maxed[nPlayer]);
				TJAPlayer3.t安全にDisposeする(ref Chara_GoGoStart[nPlayer]);
				TJAPlayer3.t安全にDisposeする(ref Chara_GoGoStart_Maxed[nPlayer]);
				TJAPlayer3.t安全にDisposeする(ref Chara_Become_Cleared[nPlayer]);
				TJAPlayer3.t安全にDisposeする(ref Chara_Become_Maxed[nPlayer]);
				TJAPlayer3.t安全にDisposeする(ref Chara_Balloon_Breaking[nPlayer]);
				TJAPlayer3.t安全にDisposeする(ref Chara_Balloon_Broke[nPlayer]);
				TJAPlayer3.t安全にDisposeする(ref Chara_Balloon_Miss[nPlayer]);
			}
			
			#endregion
			#region 踊り子
			for (int i = 0; i < Dancer.Length; i++)
			{
				TJAPlayer3.t安全にDisposeする(ref Dancer[i]);
			}
			#endregion
			#region モブ
			TJAPlayer3.t安全にDisposeする(ref Mob);
			#endregion
			#region フッター
			TJAPlayer3.t安全にDisposeする(ref Mob_Footer);
			#endregion
			#region 背景
			TJAPlayer3.t安全にDisposeする(ref Background);
			TJAPlayer3.t安全にDisposeする(ref Background_Up);
			TJAPlayer3.t安全にDisposeする(ref Background_Up_Clear);
			TJAPlayer3.t安全にDisposeする(ref Background_Up_YMove);
			TJAPlayer3.t安全にDisposeする(ref Background_Up_YMove_Clear);
			TJAPlayer3.t安全にDisposeする(ref Background_Up_Sakura);
			TJAPlayer3.t安全にDisposeする(ref Background_Up_Sakura_Clear);
			TJAPlayer3.t安全にDisposeする(ref Background_Down);
			TJAPlayer3.t安全にDisposeする(ref Background_Down_Clear);
			TJAPlayer3.t安全にDisposeする(ref Background_Down_Scroll);

			#endregion
			#region 太鼓
			TJAPlayer3.t安全にDisposeする(ref Taiko_Background);
			TJAPlayer3.t安全にDisposeする(ref Taiko_Frame);
			TJAPlayer3.t安全にDisposeする(ref Taiko_PlayerNumber);
			TJAPlayer3.t安全にDisposeする(ref Taiko_NamePlate);
			TJAPlayer3.t安全にDisposeする(ref Taiko_Base);
			TJAPlayer3.t安全にDisposeする(ref Taiko_Don_Left);
			TJAPlayer3.t安全にDisposeする(ref Taiko_Don_Right);
			TJAPlayer3.t安全にDisposeする(ref Taiko_Ka_Left);
			TJAPlayer3.t安全にDisposeする(ref Taiko_Ka_Right);
			TJAPlayer3.t安全にDisposeする(ref Taiko_LevelUp);
			TJAPlayer3.t安全にDisposeする(ref Taiko_LevelDown);
			TJAPlayer3.t安全にDisposeする(ref Couse_Symbol);			
			TJAPlayer3.t安全にDisposeする(ref Taiko_Score);
			TJAPlayer3.t安全にDisposeする(ref Taiko_Combo);
			TJAPlayer3.t安全にDisposeする(ref Taiko_Combo_Effect);
			TJAPlayer3.t安全にDisposeする(ref Taiko_Combo_Text);
			#endregion
			#region ゲージ
			TJAPlayer3.t安全にDisposeする(ref Gauge);
			TJAPlayer3.t安全にDisposeする(ref Gauge);
			TJAPlayer3.t安全にDisposeする(ref Gauge_Base);
			TJAPlayer3.t安全にDisposeする(ref Gauge_Line);
			TJAPlayer3.t安全にDisposeする(ref Gauge_Rainbow);
			TJAPlayer3.t安全にDisposeする(ref Gauge_Soul);
			TJAPlayer3.t安全にDisposeする(ref Gauge_Soul_Fire);
			TJAPlayer3.t安全にDisposeする(ref Gauge_Soul_Explosion);

            #region[Gauge_DanC]
            TJAPlayer3.t安全にDisposeする(ref Gauge_Danc);
			TJAPlayer3.t安全にDisposeする(ref Gauge_Base_Danc);
			TJAPlayer3.t安全にDisposeする(ref Gauge_Line_Danc);
			TJAPlayer3.t安全にDisposeする(ref Gauge_Rainbow_Danc);
			
            #endregion

            #endregion
            #region 吹き出し
            TJAPlayer3.t安全にDisposeする(ref Balloon_Combo);
			TJAPlayer3.t安全にDisposeする(ref Balloon_Roll);
			TJAPlayer3.t安全にDisposeする(ref Balloon_Balloon);
			TJAPlayer3.t安全にDisposeする(ref Balloon_Number_Roll);
			TJAPlayer3.t安全にDisposeする(ref Balloon_Number_Combo);
			TJAPlayer3.t安全にDisposeする(ref Balloon_Breaking);
			#endregion
			#region エフェクト
			TJAPlayer3.t安全にDisposeする(ref Effects_Hit_Explosion);
			TJAPlayer3.t安全にDisposeする(ref  Effects_Hit_Explosion_Big);
			TJAPlayer3.t安全にDisposeする(ref Effects_Hit_FireWorks);
			TJAPlayer3.t安全にDisposeする(ref Effects_Fire);
			TJAPlayer3.t安全にDisposeする(ref Effects_Rainbow);
			TJAPlayer3.t安全にDisposeする(ref Effects_GoGoSplash);
			TJAPlayer3.t安全にDisposeする(ref Effects_Hit_Great);
			TJAPlayer3.t安全にDisposeする(ref Effects_Hit_Great_Big);
			TJAPlayer3.t安全にDisposeする(ref Effects_Hit_Good);
			TJAPlayer3.t安全にDisposeする(ref Effects_Hit_Good_Big);
			TJAPlayer3.t安全にDisposeする(ref Effects_Roll);
			
			#endregion
			#region レーン
			TJAPlayer3.t安全にDisposeする(ref Lane_Base);
			TJAPlayer3.t安全にDisposeする(ref Lane_Text);
			TJAPlayer3.t安全にDisposeする(ref Lane_Red);
			TJAPlayer3.t安全にDisposeする(ref Lane_Blue);
			TJAPlayer3.t安全にDisposeする(ref Lane_Yellow);
			TJAPlayer3.t安全にDisposeする(ref Lane_Background_Main);
			TJAPlayer3.t安全にDisposeする(ref Lane_Background_Sub);
			TJAPlayer3.t安全にDisposeする(ref Lane_Background_GoGo);

			#endregion
			#region 終了演出
			TJAPlayer3.t安全にDisposeする(ref End_Clear_L);
			TJAPlayer3.t安全にDisposeする(ref End_Clear_R);
			TJAPlayer3.t安全にDisposeする(ref End_Failed_L);
			TJAPlayer3.t安全にDisposeする(ref End_Failed_R);
			TJAPlayer3.t安全にDisposeする(ref End_Fan);
			TJAPlayer3.t安全にDisposeする(ref End_Failed_Text);
			TJAPlayer3.t安全にDisposeする(ref End_Failed_Impact);
			TJAPlayer3.t安全にDisposeする(ref End_Clear_Text);
			TJAPlayer3.t安全にDisposeする(ref End_Clear_Text_Effect);
			TJAPlayer3.t安全にDisposeする(ref End_FullCombo_Text);
			TJAPlayer3.t安全にDisposeする(ref End_FullCombo_Text_Effect);
			TJAPlayer3.t安全にDisposeする(ref End_DonderFullCombo_Lane);
			TJAPlayer3.t安全にDisposeする(ref End_DonderFullCombo_L);
			TJAPlayer3.t安全にDisposeする(ref End_DonderFullCombo_R);
			TJAPlayer3.t安全にDisposeする(ref End_DonderFullCombo_Text);
			TJAPlayer3.t安全にDisposeする(ref End_DonderFullCombo_Text_Effect);
			#endregion
			#region ゲームモード
			TJAPlayer3.t安全にDisposeする(ref GameMode_Timer_Tick);
			TJAPlayer3.t安全にDisposeする(ref GameMode_Timer_Frame);
			#endregion
			#region ステージ失敗
			TJAPlayer3.t安全にDisposeする(ref Failed_Game);
			TJAPlayer3.t安全にDisposeする(ref Failed_Stage);
			#endregion
			#region ランナー
			TJAPlayer3.t安全にDisposeする(ref Runner);
			#endregion
			#region DanC
			TJAPlayer3.t安全にDisposeする(ref DanC_Background);
			TJAPlayer3.t安全にDisposeする(ref DanC_Gauge);
			TJAPlayer3.t安全にDisposeする(ref DanC_Base);
			TJAPlayer3.t安全にDisposeする(ref DanC_Failed);
			TJAPlayer3.t安全にDisposeする(ref DanC_Number);
			TJAPlayer3.t安全にDisposeする(ref DanC_ExamRange);
			TJAPlayer3.t安全にDisposeする(ref DanC_ExamUnit);
			TJAPlayer3.t安全にDisposeする(ref DanC_ExamType);
			TJAPlayer3.t安全にDisposeする(ref DanC_Screen);
			#endregion
			#region PuchiChara
			TJAPlayer3.t安全にDisposeする(ref PuchiChara);
			#endregion
			#region Training
			TJAPlayer3.t安全にDisposeする(ref Tokkun_DownBG);
			TJAPlayer3.t安全にDisposeする(ref Tokkun_BigTaiko);
			TJAPlayer3.t安全にDisposeする(ref Tokkun_ProgressBar);
			TJAPlayer3.t安全にDisposeする(ref Tokkun_ProgressBarWhite);
			TJAPlayer3.t安全にDisposeする(ref Tokkun_GoGoPoint);
			TJAPlayer3.t安全にDisposeする(ref Tokkun_JumpPoint);
			TJAPlayer3.t安全にDisposeする(ref Tokkun_Background_Up);
			TJAPlayer3.t安全にDisposeする(ref Tokkun_BigNumber);
			TJAPlayer3.t安全にDisposeする(ref Tokkun_SmallNumber);
			TJAPlayer3.t安全にDisposeする(ref Tokkun_Speed_Measure);
			#endregion
			#endregion

			#region 6_結果発表
			TJAPlayer3.t安全にDisposeする(ref Result_Background);
			TJAPlayer3.t安全にDisposeする(ref Result_FadeIn);
			TJAPlayer3.t安全にDisposeする(ref Result_Gauge);
			TJAPlayer3.t安全にDisposeする(ref Result_Gauge_Base);
			TJAPlayer3.t安全にDisposeする(ref Result_Judge);
			TJAPlayer3.t安全にDisposeする(ref Result_Header);
			TJAPlayer3.t安全にDisposeする(ref Result_Number);
			TJAPlayer3.t安全にDisposeする(ref Result_Panel);
			TJAPlayer3.t安全にDisposeする(ref Result_Score_Text);
			TJAPlayer3.t安全にDisposeする(ref Result_Score_Number);
			TJAPlayer3.t安全にDisposeする(ref Result_Dan);
			#endregion

			#region 7_終了画面
			TJAPlayer3.t安全にDisposeする(ref Exit_Background);
			TJAPlayer3.t安全にDisposeする(ref Exit_Curtain);
			TJAPlayer3.t安全にDisposeする(ref Exit_Text);
			#endregion

		}

		private static CTexture GenerateTitleTexture(TitleTextureKey titleTextureKey)
		{
			using (var bmp = new Bitmap(titleTextureKey.cPrivateFastFont.DrawPrivateFont(
				titleTextureKey.str文字, titleTextureKey.forecolor, titleTextureKey.backcolor, true)))
			{
				CTexture tx文字テクスチャ = TJAPlayer3.tテクスチャの生成(bmp);
				if (tx文字テクスチャ.szテクスチャサイズ.Height > titleTextureKey.maxHeight)
				{
					tx文字テクスチャ.vc拡大縮小倍率.Y = (float)(((double)titleTextureKey.maxHeight) / tx文字テクスチャ.szテクスチャサイズ.Height);
				}

				return tx文字テクスチャ;
			}
		}
		private sealed class TitleTextureKey
		{
			public readonly string str文字;
			public readonly CPrivateFastFont cPrivateFastFont;
			public readonly Color forecolor;
			public readonly Color backcolor;
			public readonly int maxHeight;

			public TitleTextureKey(string str文字, CPrivateFastFont cPrivateFastFont, Color forecolor, Color backcolor, int maxHeight)
			{
				this.str文字 = str文字;
				this.cPrivateFastFont = cPrivateFastFont;
				this.forecolor = forecolor;
				this.backcolor = backcolor;
				this.maxHeight = maxHeight;
			}

			private bool Equals(TitleTextureKey other)
			{
				return string.Equals(str文字, other.str文字) &&
					   cPrivateFastFont.Equals(other.cPrivateFastFont) &&
					   forecolor.Equals(other.forecolor) &&
					   backcolor.Equals(other.backcolor) &&
					   maxHeight == other.maxHeight;
			}

			public override bool Equals(object obj)
			{
				if (ReferenceEquals(null, obj)) return false;
				if (ReferenceEquals(this, obj)) return true;
				return obj is TitleTextureKey other && Equals(other);
			}

			public override int GetHashCode()
			{
				unchecked
				{
					var hashCode = str文字.GetHashCode();
					hashCode = (hashCode * 397) ^ cPrivateFastFont.GetHashCode();
					hashCode = (hashCode * 397) ^ forecolor.GetHashCode();
					hashCode = (hashCode * 397) ^ backcolor.GetHashCode();
					hashCode = (hashCode * 397) ^ maxHeight;
					return hashCode;
				}
			}
		}
		private TitleTextureKey ttk曲名テクスチャを生成する(string str文字, Color forecolor, Color backcolor)
		{
			return new TitleTextureKey(str文字, new CPrivateFastFont(new FontFamily(TJAPlayer3.ConfigIni.FontName), 28), forecolor, backcolor, 410);
		}

		#region 共通
		public CTexture Tile_Black,
			Menu_Title,
			Menu_Highlight,
			Enum_Song,
			Scanning_Loudness,
			Overlay,
			Overlay_Offline,
			Overlay_Online,
			Crown_t,
			DanC_Crown_t,
			Difficulty_Icons;
		public CTexture[] NamePlate;
		#endregion
		#region 1_タイトル画面
		public CTexture Title_Background,
			Title_Menu,
			Title_AcBar,
			Title_InBar;
		public CTexture[] Title_Txt = new CTexture[3];
		#endregion

		#region 2_コンフィグ画面
		public CTexture Config_Background,
			Config_Cursor,
			Config_ItemBox,
			Config_Arrow,
			Config_KeyAssign,
			Config_Font,
			Config_Font_Bold,
			Config_Enum_Song;
		#endregion

		#region 3_選曲画面
		public CTexture SongSelect_Background,
			SongSelect_Header,
			SongSelect_Footer,
			SongSelect_Difficulty,
			SongSelect_Auto,
			SongSelect_Level,
			SongSelect_Branch,
			SongSelect_Branch_Text,
			SongSelect_Branch_Text_NEW,
			SongSelect_Frame_Score,
			SongSelect_Frame_Box,
			SongSelect_Frame_BackBox,
			SongSelect_Frame_Random,
			SongSelect_Bar_Center,
			SongSelect_Cursor_Left,
			SongSelect_Cursor_Right,
			SongSelect_ScoreWindow_Text,
			SongSelect_Bar_BackBox;
		public CTexture[] SongSelect_GenreBack = new CTexture[9],
			SongSelect_ScoreWindow = new CTexture[(int)Difficulty.Total],
			SongSelect_Lyric_Text = new CTexture[9],
			SongSelect_Box_Center_Genre = new CTexture[9],
			SongSelect_Box_Center_Header_Genre = new CTexture[9],
			SongSelect_Box_Center_Text_Genre = new CTexture[9],
			SongSelect_Bar_Center_Back_Genre = new CTexture[9],
			SongSelect_Bar_Genre = new CTexture[9],
			SongSelect_Bar_Box_Genre = new CTexture[9],
			SongSelect_NamePlate = new CTexture[1],
			SongSelect_Counter_Back = new CTexture[2],
			SongSelect_Counter_Num = new CTexture[2];
		#region[3.5_難易度選択]
		public CTexture Difficulty_Dan_Box,
			Difficulty_Dan_Box_Selecting,
			Difficulty_Star,
			Difficulty_Branch,
			Difficulty_Center_Bar,
			ChangeSE_Box,
			ChangeSE_Note,
			ChangeSE_Num,
			PlayOption_List,
			PlayOption_Active;
		public CTexture[] Difficulty_Bar = new CTexture[5],
			Difficulty_Bar_Etc = new CTexture[3],
			Difficulty_Anc = new CTexture[2],
			Difficulty_Anc_Same = new CTexture[2],
			Difficulty_Anc_Box = new CTexture[2],
			Difficulty_Anc_Box_Etc = new CTexture[2],
			Difficulty_Mark = new CTexture[5];
		#endregion

		#endregion

		#region 4_読み込み画面
		public CTexture SongLoading_Plate,
			SongLoading_FadeIn,
			SongLoading_FadeOut;
		#endregion

		#region 5_演奏画面
		#region 共通
		public CTexture Notes,
			Notes_White,
			Judge_Frame,
			SENotes,
			Notes_Arm,
			Judge;
		public CTexture Judge_Meter,
			Bar,
			Bar_Branch;
		#endregion
		#region キャラクター
		public CTexture[][] Chara_Normal = new CTexture[2][],
			Chara_Normal_Cleared = new CTexture[2][],
			Chara_Normal_Maxed = new CTexture[2][],
			Chara_GoGoTime = new CTexture[2][],
			Chara_GoGoTime_Maxed = new CTexture[2][],
			Chara_10Combo = new CTexture[2][],
			Chara_10Combo_Maxed = new CTexture[2][],
			Chara_GoGoStart = new CTexture[2][],
			Chara_GoGoStart_Maxed = new CTexture[2][],
			Chara_Become_Cleared = new CTexture[2][],
			Chara_Become_Maxed = new CTexture[2][],
			Chara_Balloon_Breaking = new CTexture[2][],
			Chara_Balloon_Broke = new CTexture[2][],
			Chara_Balloon_Miss = new CTexture[2][];
		#endregion
		#region 踊り子
		public CTexture[][] Dancer;
		#endregion
		#region モブ
		public CTexture[] Mob;
		public CTexture Mob_Footer;
		#endregion
		#region 背景
		public CTexture Background,
			Background_Down,
			Background_Down_Clear,
			Background_Down_Scroll;
		public CTexture[] Background_Up,
			Background_Up_Clear,
			Background_Up_YMove,
			Background_Up_YMove_Clear,
			Background_Up_Sakura,
			Background_Up_Sakura_Clear;
		#endregion
		#region 太鼓
		public CTexture[] Taiko_Frame, // MTaiko下敷き
			Taiko_Background;
		public CTexture Taiko_Base,
			Taiko_Don_Left,
			Taiko_Don_Right,
			Taiko_Ka_Left,
			Taiko_Ka_Right,
			Taiko_LevelUp,
			Taiko_LevelDown,
			Taiko_Combo_Effect,
			Taiko_Combo_Text;
		public CTexture[] Couse_Symbol, // コースシンボル
			Taiko_PlayerNumber,
			Taiko_NamePlate; // ネームプレート
		public CTexture[] Taiko_Score,
			Taiko_Combo;
		#endregion
		#region ゲージ
		public CTexture[] Gauge,
			Gauge_Base,
			Gauge_Line,
			Gauge_Rainbow,
			Gauge_Soul_Explosion,
			Gauge_Rainbow_Danc;
		public CTexture Gauge_Soul,
			Gauge_Danc,
			Gauge_Base_Danc,
			Gauge_Line_Danc,
			Gauge_Soul_Fire;
		#endregion
		#region 吹き出し
		public CTexture[] Balloon_Combo;
		public CTexture Balloon_Roll,
			Balloon_Balloon,
			Balloon_Number_Roll,
			Balloon_Number_Combo/*,*/
								/*Balloon_Broken*/;
		public CTexture[] Balloon_Breaking;
		#endregion
		#region エフェクト
		public CTexture Effects_Hit_Explosion,
			Effects_Hit_Explosion_Big,
			Effects_Fire,
			Effects_Rainbow,
			Effects_GoGoSplash,
			Effects_Hit_FireWorks;
		public CTexture[] Effects_Hit_Great,
			Effects_Hit_Good,
			Effects_Hit_Great_Big,
			Effects_Hit_Good_Big;
		public CTexture[] Effects_Roll;
		#endregion
		#region レーン
		public CTexture[] Lane_Base,
			Lane_Text;
		public CTexture Lane_Red,
			Lane_Blue,
			Lane_Yellow;
		public CTexture Lane_Background_Main,
			Lane_Background_Sub,
			Lane_Background_GoGo;
		#endregion
		#region 終了演出
		public CTexture[] End_Failed_L,
			End_Failed_R, 
			End_Clear_L,
			End_Clear_R, 
			End_Fan;
		public CTexture End_Failed_Text,
			End_Failed_Impact,
			End_Clear_Text,
			End_Clear_Text_Effect,
			End_FullCombo_Text,
			End_FullCombo_Text_Effect,
			End_DonderFullCombo_Lane,
			End_DonderFullCombo_L,
			End_DonderFullCombo_R,
			End_DonderFullCombo_Text,
			End_DonderFullCombo_Text_Effect;
		#endregion
		#region ゲームモード
		public CTexture GameMode_Timer_Frame,
			GameMode_Timer_Tick;
		#endregion
		#region ステージ失敗
		public CTexture Failed_Game,
			Failed_Stage;
		#endregion
		#region ランナー
		public CTexture Runner;
		#endregion
		#region DanC
		public CTexture DanC_Background;
		public CTexture[] DanC_Gauge;
		public CTexture DanC_Base;
		public CTexture DanC_Failed;
		public CTexture DanC_Number,
			DanC_ExamType,
			DanC_ExamRange,
			DanC_ExamUnit;
		public CTexture DanC_Screen;
		#endregion
		#region PuchiChara
		public CTexture[] PuchiChara;
		#endregion
		#region Training
		public CTexture Tokkun_DownBG,
			Tokkun_BigTaiko,
			Tokkun_ProgressBar,
			Tokkun_ProgressBarWhite,
			Tokkun_GoGoPoint,
			Tokkun_JumpPoint,
			Tokkun_Background_Up,
			Tokkun_BigNumber,
			Tokkun_SmallNumber,
			Tokkun_Speed_Measure;
		#endregion
		#endregion

		#region 6_結果発表
		public CTexture Result_Background,
			Result_FadeIn,
			Result_Gauge,
			Result_Gauge_Base,
			Result_Judge,
			Result_Header,
			Result_Number,
			Result_Panel,
			Result_Score_Text,
			Result_Score_Number,
			Result_Dan;
		#endregion

		#region 7_終了画面
		public CTexture Exit_Background,
						Exit_Curtain,
						Exit_Text;
		#endregion

	}
}
