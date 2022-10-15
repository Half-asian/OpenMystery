using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;
using System.Text;
using System;
using static ConfigAvatarComponents;

public class AvatarComponents
{
    public static event Action onReapplyModifiers = delegate { };

    public Dictionary<string, AnimationManager.BoneMod> bonemods;

    public string actor_id;
    public string gender;

    public Dictionary<string, AvatarComponent> components;

    public ActorController character;

    public Model base_model = null;
    public Dictionary<string, PlayerFile.CustomizationCategory> customization_categories;

    public static List<string> avatar_components_hair; 
    public static List<string> avatar_components_one_piece;
    public static List<string> avatar_components_tops;
    public static List<string> avatar_components_bottoms;
    public static List<string> avatar_components_eyes;
    public static List<string> avatar_components_lips;
    public static List<string> avatar_components_facepaint;


    public AvatarComponents(string filename)
    {
        components = new Dictionary<string, AvatarComponent>();
        bonemods = new Dictionary<string, AnimationManager.BoneMod>();
        actor_id = PlayerManager.current.character_id;

        resetFromPlayerFile();
    }
    
    public void callReapply()
    {
        onReapplyModifiers.Invoke();
    }

    public void resetFromPlayerFile()
    {
        customization_categories = new Dictionary<string, PlayerFile.CustomizationCategory>();
        foreach (string key in PlayerManager.current.customization_categories.Keys)
        {
            customization_categories[key] = new PlayerFile.CustomizationCategory();
            customization_categories[key].component_id = PlayerManager.current.customization_categories[key].component_id;
            if (PlayerManager.current.customization_categories[key].float_parameters != null)
            {
                customization_categories[key].float_parameters = new Dictionary<string, float>();
                foreach (string fkey in PlayerManager.current.customization_categories[key].float_parameters.Keys)
                {
                    customization_categories[key].float_parameters[fkey] = PlayerManager.current.customization_categories[key].float_parameters[fkey];
                }
            }
            if (PlayerManager.current.customization_categories[key].int_parameters != null)
            {
                customization_categories[key].int_parameters = new Dictionary<string, int>();
                foreach (string fkey in PlayerManager.current.customization_categories[key].int_parameters.Keys)
                {
                    customization_categories[key].int_parameters[fkey] = PlayerManager.current.customization_categories[key].int_parameters[fkey];
                }
            }
        }

        if (!customization_categories.ContainsKey("legs"))
        {
            customization_categories["legs"] = new PlayerFile.CustomizationCategory();
            customization_categories["legs"].component_id = "LongPants";
            //components["legs"] = new IndividualComponents.ComponentLegs(this);
            replaceAvatarLegs("LongPants");
        }
        if (!customization_categories.ContainsKey("arms"))
        {
            customization_categories["arms"] = new PlayerFile.CustomizationCategory();
            customization_categories["arms"].component_id = "LongSleeve";
            //components["arms"] = new IndividualComponents.ComponentArms(this);
            replaceAvatarArms("LongSleeve");
        }

        foreach (var comp in components)
        {
            comp.Value.replaceComponent();
        }
    }

    public void commitChanges()
    {
        PlayerManager.current.customization_categories = customization_categories;
    }

    public void undoChanges()
    {
        resetFromPlayerFile();
        throw new System.NotImplementedException();
    }

    public void setCharacterManager(ActorController _character)
    {
        character = _character;
    }

    public void equipAvatarComponent(string component_name)
    {
        Debug.Log("equipAvatarComponent " + component_name);
        string category = Configs.config_avatar_components.AvatarComponents[component_name].category;
        if (!customization_categories.ContainsKey(category))
        {
            customization_categories[category] = new PlayerFile.CustomizationCategory();
        }
        customization_categories[category].component_id = component_name;

        spawnComponent(category);
    }

    public void spawnComponents()
    {
        components["hands"] = new IndividualComponents.ComponentHands(this);

        foreach (string category in PlayerManager.current.customization_categories.Keys)
        {
            spawnComponent(category);
        }
    }

    void spawnComponent(string category)
    {
        switch (category)
        {
            case "brows":
                if (components.ContainsKey("brows"))
                    components["brows"].replaceComponent();
                else
                    components["brows"] = new IndividualComponents.ComponentBrows(this);
                break;
            case "eyes":
                if (components.ContainsKey("eyes"))
                    components["eyes"].replaceComponent();
                else
                    components["eyes"] = new IndividualComponents.ComponentEyes(this);
                break;
            case "faces":
                if (components.ContainsKey("faces"))
                    components["faces"].replaceComponent();
                else
                    components["faces"] = new IndividualComponents.ComponentFaces(this);
                break;
            case "nose":
                if (components.ContainsKey("nose"))
                    components["nose"].replaceComponent();
                else
                    components["nose"] = new IndividualComponents.ComponentNose(this);
                break;
            case "lips":
                if (components.ContainsKey("lips"))
                    components["lips"].replaceComponent();
                else
                    components["lips"] = new IndividualComponents.ComponentLips(this);
                break;
            case "hair":
                if (components.ContainsKey("hair"))
                    components["hair"].replaceComponent();
                else
                    components["hair"] = new IndividualComponents.ComponentHair(this);
                break;
            case "glasses":
                if (components.ContainsKey("glasses"))
                    components["glasses"].replaceComponent();
                else
                    components["glasses"] = new IndividualComponents.ComponentGlasses(this);
                break;
            case "one-piece":
                if (components.ContainsKey("tops"))
                {
                    components["tops"].removeComponent();
                    components.Remove("tops");
                    customization_categories.Remove("tops");
                }
                if (components.ContainsKey("bottoms"))
                {
                    components["bottoms"].removeComponent();
                    components.Remove("bottoms");
                    customization_categories.Remove("bottoms");
                }
                if (components.ContainsKey("one-piece"))
                    components["one-piece"].replaceComponent();
                else
                    components["one-piece"] = new IndividualComponents.ComponentOnePiece(this);
                break;
            case "tops":
                if (components.ContainsKey("one-piece"))
                {
                    components["one-piece"].removeComponent();
                    components.Remove("one-piece");
                    customization_categories.Remove("one-piece");
                }

                if (!components.ContainsKey("bottoms"))
                {
                    components["bottoms"] = new IndividualComponents.ComponentBottom(this);
                }
                else { }

                if (components.ContainsKey("tops"))
                    components["tops"].replaceComponent();
                else
                    components["tops"] = new IndividualComponents.ComponentTop(this);
                break;

            case "neckwear":
                if (components.ContainsKey("neckwear"))
                    components["neckwear"].replaceComponent();
                else
                    components["neckwear"] = new IndividualComponents.ComponentNeckwear(this);
                break;
            case "facePaint":
                if (components.ContainsKey("facePaint"))
                    components["facePaint"].replaceComponent();
                else
                    components["facePaint"] = new IndividualComponents.ComponentFacepaint(this);
                break;
            case "bottoms":
                if (components.ContainsKey("one-piece"))
                {
                    components["one-piece"].removeComponent();
                    components.Remove("one-piece");
                    customization_categories.Remove("one-piece");
                }

                if (!components.ContainsKey("tops"))// && customization_categories.ContainsKey("tops"))
                {
                    components["tops"] = new IndividualComponents.ComponentTop(this);
                }

                if (components.ContainsKey("bottoms"))
                    components["bottoms"].replaceComponent();
                else
                    components["bottoms"] = new IndividualComponents.ComponentBottom(this);
                break;
            case "right-wrist":
                if (components.ContainsKey("right-wrist"))
                    components["right-wrist"].replaceComponent();
                else
                    components["right-wrist"] = new IndividualComponents.ComponentRightWrist(this);
                break;
            case "legs":
                if (components.ContainsKey("legs"))
                    components["legs"].replaceComponent();
                else
                    components["legs"] = new IndividualComponents.ComponentLegs(this);
                break;
        }
    }

    public void changeAvatarEyeColor(int colorId)
    {
        components["eyes"].setInt(colorId, "eyeColor");
    }

    public void changeAvatarSkinColor(int colorId)
    {
        customization_categories["faces"].int_parameters["skinColor"] = colorId;
        callReapply();
    }
    public void changeAvatarLipsColor(int colorId)
    {
        components["lips"].setInt(colorId, "naturalLips");
    }
    public void changeAvatarBrowColor(int colorId)
    {
        customization_categories["brows"].int_parameters["browColor"] = colorId;
        callReapply();
    }
    public void changeAvatarHairColor(int colorId)
    {
        components["hair"].setInt(colorId, "hairColor");
    }

    public void changeAvatarBrowThickness(float browThickness) { components["brows"].setFloat(browThickness, "browThickness"); }
    public void changeAvatarEyeCloseness(float eyeCloseness) { components["eyes"].setFloat(eyeCloseness, "eyeCloseness"); }
    public void changeAvatarEyeSize(float eyeSize) { components["eyes"].setFloat(eyeSize, "eyeSize"); }
    public void changeAvatarEyeY(float eyeY) { components["eyes"].setFloat(eyeY, "eyeY"); }
    public void changeAvatarChinSize(float chinSize) { components["faces"].setFloat(chinSize, "chinSize"); }
    public void changeAvatarJawSize(float chinSize) { components["faces"].setFloat(chinSize, "jawSize"); }
    public void changeAvatarNoseBridgeHeight(float noseBridgeHeight) { components["nose"].setFloat(noseBridgeHeight, "noseBridgeHeight"); }
    public void changeAvatarNoseBridgeLength(float noseBridgeLength) { components["nose"].setFloat(noseBridgeLength, "noseBridgeLength"); }
    public void changeAvatarNoseBridgeWidth(float noseBridgeWidth) { components["nose"].setFloat(noseBridgeWidth, "noseBridgeWidth"); }
    public void changeAvatarNoseFatness(float noseFatness) { components["nose"].setFloat(noseFatness, "noseFatness"); }
    public void changeAvatarNoseHeight(float noseHeight) { components["nose"].setFloat(noseHeight, "noseHeight"); }
    public void changeAvatarNoseWidth(float noseWidth) { components["nose"].setFloat(noseWidth, "noseWidth"); }
    public void changeAvatarNoseLength(float noseLength) { components["nose"].setFloat(noseLength, "noseLength"); }
    public void changeAvatarNoseTwist(float noseTwist) { components["nose"].setFloat(noseTwist, "noseTwist"); }

    public void replaceAvatarArms(string component_id)
    {
        if (!customization_categories.ContainsKey("arms") || !components.ContainsKey("arms"))
        {
            customization_categories["arms"] = new PlayerFile.CustomizationCategory();
            if (component_id == null)
                component_id = "LongSleeve";
            customization_categories["arms"].component_id = component_id;
            components["arms"] = new IndividualComponents.ComponentArms(this);

        }
        else
        {
            customization_categories["arms"].component_id = component_id;
            components["arms"].replaceComponent();
        }
    }

    public void replaceAvatarLegs(string component_id)
    {
        if (!customization_categories.ContainsKey("legs") || !components.ContainsKey("legs"))
        {
            customization_categories["legs"] = new PlayerFile.CustomizationCategory();
            customization_categories["legs"].component_id = component_id;
            components["legs"] = new IndividualComponents.ComponentLegs(this);

        }
        else
        {
            customization_categories["legs"].component_id = component_id;
            components["legs"].replaceComponent();
        }
    }

}

public abstract class AvatarComponent
{
    protected Model component_model;
    protected AvatarComponents avatar_components;

    public virtual void setFloat(float f, string s)
    {
        throw new System.NotImplementedException();
    }
    public virtual void setInt(int i, string s)
    {
        throw new System.NotImplementedException();
    }
    public virtual void setModifiers()
    {
    }
    public virtual Model getModel()
    {
        return null;
    }
    public virtual Model replaceComponent()
    {
        return null;
    }
    public virtual void removeComponent()
    {
    }
    public virtual void hideComponent()
    {
    }
    public virtual void showComponent()
    {
    }
}

public abstract class AvatarComponentWithModel : AvatarComponent
{
    public override Model getModel()
    {
        return component_model;
    }
    public override void removeComponent()
    {
        AvatarComponents.onReapplyModifiers -= setModifiers;
        GameObject.DestroyImmediate(component_model.game_object);
        component_model = null;
    }
    public override void hideComponent()
    {
        if (component_model != null)
        {
            foreach (SkinnedMeshRenderer smr in component_model.game_object.GetComponentsInChildren<SkinnedMeshRenderer>())
                smr.enabled = false;
        }
    }
    public override void showComponent()
    {
        if (component_model != null)
        {
            foreach (SkinnedMeshRenderer smr in component_model.game_object.GetComponentsInChildren<SkinnedMeshRenderer>())
                smr.enabled = true;
        }
    }

}
