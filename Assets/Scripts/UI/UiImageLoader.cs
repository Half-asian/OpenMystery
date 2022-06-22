using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
public class UiImageLoader : MonoBehaviour
{
    /*Game Loading Images from APK*/

    public Image loading_screen_background_image;
    public Image loading_screen_spinner_image;

    public Material hp_hud_important_material;
    public Material hp_icon_tooltip_material;
    public Material hp_nav_exit_material;

    public Image dialogue1; //hp_container_dialogue@4x.png
    public Image dialogue2; //hp_bg_container_hud_name@4x.png
    public Image dialogue3; //hp_bg_dialogue_name_gradient@4x.png
    public Image dialogue_choice1; //hp_button_dialogue@4x.png
    public Image dialogue_choice2; //hp_button_dialogue@4x.png
    public Image dialogue_choice3; //hp_button_dialogue@4x.png

    public Image menu_quit_game;
    public Image menu_settings;
    public Image menu_chapter_select;
    public Image next_area;
    public Image exit_to_menu;

    public Image menu_mq_1; //hp_bg_gradient_linear_blue@2x.png
    public Image menu_mq_2; //hp_bg_modal_divider_2@4x.png
    public Image menu_mq_3; //hp_bg_modal_divider_2@4x.png
    public Image menu_mq_4; //hp_icon_hogwarts_castle@4x.png
    public Image menu_mq_button; //hp_btn_cta_med@4x.png

    public Image menu_tlsq_box; //"hp_modal_bg_lrg@4x.png"
    public Image menu_tlsq_1; //hp_bg_modal_divider_2@4x.png
    public Image menu_tlsq_2; //hp_bg_modal_header_sm@4x.png
    public Image menu_tlsq_3; //hp_icon_notif_timer@2x.png
    public Image menu_tlsq_button; //hp_btn_cta_med@4x.png

    public Image menu_settings_box;

    public Image goal_popup;
    public Image goal_popup_character_bg;
    public Image goal_popup_image2;


    public IEnumerator setup()
    {
        //yield return null;

        //The fonts are open source lol
        /*Font eb_garamond12_regular = new Font(GlobalEngineVariables.apk_folder + "\\assets\\EBGaramond12-Regular.ttf");
        TMP_FontAsset font_asset = TMP_FontAsset.CreateFontAsset(eb_garamond12_regular);
        press_space_text.font = font_asset;
        please_wait_text.font = font_asset;*/

        yield return null;
        //loading screen backround
        loading_screen_background_image.sprite = loadSpriteFromApk("hp_loadingscreen_ex@4x.jpg");
        yield return null;
        //loading screen spinner
        loading_screen_spinner_image.sprite = loadSpriteFromApk("hp_img_halo@4x.png");
        yield return null;
        //hud important
        hp_hud_important_material.SetTexture("Icon", loadTextureFromApk("hp_hud_important_sml_tooltip@4x.png"));
        yield return null;
        //hud tooltip
        hp_icon_tooltip_material.SetTexture("Icon", loadTextureFromApk("hp_icon_tooltip_talk@4x.png"));
        yield return null;
        //hud exit
        hp_nav_exit_material.SetTexture("Icon", loadTextureFromApk("hp_nav_exit_sml@4x.png"));
        yield return null;

        //Dialogue 
        dialogue1.sprite = loadSpriteFromApk("hp_container_dialogue@4x.png");
        yield return null;
        dialogue2.sprite = loadSpriteFromApk("hp_bg_container_hud_name@4x.png");
        yield return null;
        dialogue3.sprite = loadSpriteFromApk("hp_bg_dialogue_name_gradient@4x.png");
        yield return null;
        dialogue_choice1.sprite = loadSpriteFromApk("hp_button_dialogue@4x.png");
        dialogue_choice2.sprite = dialogue_choice1.sprite;
        dialogue_choice3.sprite = dialogue_choice1.sprite;
        //Menu
        menu_quit_game.sprite = dialogue_choice1.sprite;
        next_area.sprite = dialogue_choice1.sprite;
        exit_to_menu.sprite = dialogue_choice1.sprite;
        menu_settings.sprite = dialogue_choice1.sprite;
        menu_chapter_select.sprite = dialogue_choice1.sprite;
        //Menu MQ
        yield return null;
        menu_mq_1.sprite = loadSpriteFromApk("hp_bg_gradient_linear_blue@2x.png");
        yield return null;
        menu_mq_2.sprite = loadSpriteFromApk("hp_bg_modal_divider_2@4x.png");
        menu_mq_3.sprite = menu_mq_2.sprite;
        yield return null;
        menu_mq_4.sprite = loadSpriteFromApk("hp_icon_hogwarts_castle@4x.png");
        yield return null;
        menu_mq_button.sprite = loadSpriteFromApk("hp_btn_cta_med@4x.png");
        //Menu TLSQ
        menu_tlsq_box.sprite = loadSpriteFromApk("hp_modal_bg_lrg@4x.png");
        menu_tlsq_1.sprite = menu_mq_2.sprite;
        yield return null;
        menu_tlsq_2.sprite = loadSpriteFromApk("hp_bg_modal_header_sm@4x.png");
        yield return null;
        menu_tlsq_3.sprite = loadSpriteFromApk("hp_icon_notif_timer@2x.png");
        menu_tlsq_button.sprite = menu_mq_button.sprite;

        menu_settings_box.sprite = menu_tlsq_box.sprite;

        yield return null;
        goal_popup.sprite = loadSpriteFromApk("hp_bg_notif_lrg@4x.png");
        yield return null;
        goal_popup_character_bg.sprite = loadSpriteFromApk("hp_container_char_sm@4x.png");
        yield return null;
        goal_popup_image2.sprite = loadSpriteFromApk("hp_container_unlocked_inactive@4x.png");
    }

    Texture2D loadTextureFromApk(string filename)
    {
        using (FileStream fs = File.Open(GlobalEngineVariables.apk_folder + "\\assets\\" + filename, FileMode.Open))
        {
            byte[] data = new BinaryReader(fs).ReadBytes((int)fs.Length);
            Texture2D tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            tex.LoadImage(data);
            tex.name = name;
            return tex;
        }
    }

    Sprite loadSpriteFromApk(string filename)
    {
        using (FileStream fs = File.Open(GlobalEngineVariables.apk_folder + "\\assets\\" + filename, FileMode.Open))
        {
            byte[] data = new BinaryReader(fs).ReadBytes((int)fs.Length);
            Texture2D tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            tex.LoadImage(data);
            tex.name = name;
            Sprite sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), Vector2.one * 0.5f, 100.0f, 0, SpriteMeshType.FullRect);
            return sprite;
        }
    }
}
