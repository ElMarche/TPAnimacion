using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public static class GruntAnimatorSetup
{
    private const string SetupSessionKey = "GruntAnimatorSetup.Completed";

    [InitializeOnLoadMethod]
    private static void ScheduleRebuild()
    {
        if (SessionState.GetBool(SetupSessionKey, false)) return;

        SessionState.SetBool(SetupSessionKey, true);
        EditorApplication.delayCall += Rebuild;
    }

    public static void Rebuild()
    {
        const string controllerPath = "Assets/Animations/Grunt.controller";

        AnimationClip idleClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(
            "Assets/Animations/GruntAnimation.anim"
        );
        AnimationClip walkClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(
            "Assets/Animations/GruntWalk.anim"
        );
        AnimationClip shootClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(
            "Assets/Animations/GruntShoot.anim"
        );
        AnimationClip deathClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(
            "Assets/Animations/GruntDeath.anim"
        );

        AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
        if (controller == null)
        {
            controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
        }

        while (controller.parameters.Length > 0)
        {
            controller.RemoveParameter(0);
        }

        controller.AddParameter("Walking", AnimatorControllerParameterType.Bool);
        controller.AddParameter("Shooting", AnimatorControllerParameterType.Bool);
        controller.AddParameter("Die", AnimatorControllerParameterType.Trigger);

        AnimatorStateMachine stateMachine = controller.layers[0].stateMachine;
        foreach (ChildAnimatorState childState in stateMachine.states)
        {
            stateMachine.RemoveState(childState.state);
        }

        foreach (AnimatorStateTransition transition in stateMachine.anyStateTransitions)
        {
            stateMachine.RemoveAnyStateTransition(transition);
        }

        AnimatorState idle = stateMachine.AddState("GruntIdle", new Vector3(250.0f, 100.0f));
        AnimatorState walk = stateMachine.AddState("GruntWalk", new Vector3(480.0f, 40.0f));
        AnimatorState shoot = stateMachine.AddState("GruntShoot", new Vector3(480.0f, 180.0f));
        AnimatorState death = stateMachine.AddState("GruntDeath", new Vector3(700.0f, 180.0f));

        idle.motion = idleClip;
        walk.motion = walkClip;
        shoot.motion = shootClip;
        death.motion = deathClip;
        stateMachine.defaultState = idle;

        AddBoolTransition(idle, walk, "Walking", true);
        AddBoolTransition(walk, idle, "Walking", false);
        AddBoolTransition(idle, shoot, "Shooting", true);
        AddBoolTransition(walk, shoot, "Shooting", true);
        AddBoolTransition(shoot, walk, "Shooting", false, "Walking", true);
        AddBoolTransition(shoot, idle, "Shooting", false, "Walking", false);

        AnimatorStateTransition deathTransition = stateMachine.AddAnyStateTransition(death);
        deathTransition.AddCondition(AnimatorConditionMode.If, 0.0f, "Die");
        ConfigureTransition(deathTransition);

        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Grunt Animator rebuilt successfully.");
    }

    private static void AddBoolTransition(
        AnimatorState source,
        AnimatorState destination,
        string parameter,
        bool value,
        string secondParameter = null,
        bool secondValue = false
    )
    {
        AnimatorStateTransition transition = source.AddTransition(destination);
        transition.AddCondition(
            value ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot,
            0.0f,
            parameter
        );

        if (!string.IsNullOrEmpty(secondParameter))
        {
            transition.AddCondition(
                secondValue ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot,
                0.0f,
                secondParameter
            );
        }

        ConfigureTransition(transition);
    }

    private static void ConfigureTransition(AnimatorStateTransition transition)
    {
        transition.hasExitTime = false;
        transition.duration = 0.05f;
        transition.canTransitionToSelf = false;
    }
}
