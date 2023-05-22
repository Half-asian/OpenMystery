public class InteractionMemoryMatchQTE : Interaction
{
    public override Interaction setup(ref ConfigInteraction.Interaction _interaction)
    {
        base.setup(ref _interaction);
        interaction_gameobject.SetActive(false);
        activate();
        return this;
    }

    public override void onFinishedEnterEvents()
    {
        base.onFinishedEnterEvents();
        interactionComplete();
    }

}
