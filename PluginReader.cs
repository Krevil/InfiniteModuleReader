using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.InteropServices;
using OodleSharp;


namespace InfiniteModuleReader
{
    class PluginReader
    {

    }

    public struct PluginItem
    {
        public string Name;
        public PluginField pluginField;
        public int Offset;
    }

    public enum PluginField
    {
        Comment,
        Flags8,
        Flags16,
        Flags32,
        Flags64,
        Enum8,
        Enum16,
        Enum32,
        Enum64,
        Int8,
        Int16,
        Int32,
        Int64,
        Float,
        Double,
        Float2,
        Float3,
        Float4,
        StringID,
        TagReference,
        TagBlock,
        TagStruct,
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct GrappleHookDefinition
    {
        [FieldOffset(0)]
        public long vtable_space;

        [FieldOffset(8)]
        public int global_tag_id;

        [FieldOffset(12)]
        public int local_tag_handle;

        [FieldOffset(16)]
        public int input;
        /*
        public enum input : uint
        {
            Default,
            Mobility,
            Suit,
            Jump,
            Equipment,
            Melee,
            PowerUp,
            Unmapped
        }
        */
        [FieldOffset(20)]
        public Flags32 flags;

        [FieldOffset(24)]
        public float activation_energy_cost;

        [FieldOffset(28)]
        public float active_energy_consumption;

        [FieldOffset(32)]
        public float recharge_duration;

        [FieldOffset(36)]
        public float recharge_delay;

        [FieldOffset(40)]
        public float cooldown_delay;

        [FieldOffset(44)]
        public TagReference activation_effect;

        [FieldOffset(72)]
        public float activation_effect_kill_delay;

        [FieldOffset(76)]
        public byte sprint_preservation;
        /*
        public enum sprint_preservation : byte
        {
            No,
            Zero_fraction,
            Retain_fraction,
            Decay_normal,
            Decay_forced
        }
        */
        [FieldOffset(77)]
        public short pad1;

        [FieldOffset(79)]
        public byte pad2;

        [FieldOffset(80)]
        public TagReference active_malleable_property_modifiers;

        [FieldOffset(108)]
        public TagReference timed_malleable_property_modifiers;

        [FieldOffset(136)]
        public Flags32 grapple_hook_flags;

        [FieldOffset(140)]
        public int first_person_animation_set; //stringID

        [FieldOffset(144)]
        public float max_aim_vector_angle; //angle

        [FieldOffset(148)]
        public int animation_yaw_and_pitch_origin_marker; //stringID

        [FieldOffset(152)]
        public TagReference rope_beam_effect;

        [FieldOffset(180)]
        public int rope_beam_effect_start_marker; //stringID

        [FieldOffset(184)]
        public TagReference pulling_effect;

        [FieldOffset(212)]
        public TagReference reeling_in_effect;

        [FieldOffset(240)]
        public TagReference deactivate_effect;

        [FieldOffset(268)]
        public TagReference hook_projectile;

        [FieldOffset(296)]
        public int hook_projectile_spawn_marker;

        [FieldOffset(300)]
        public TagReference hook_projectile_spawn_object;

        [FieldOffset(328)]
        public RealVector3D hook_projectile_spawn_offset;

        [FieldOffset(340)]
        public float warmup_time;

        [FieldOffset(344)]
        public float post_deploy_transition_delay;

        [FieldOffset(348)]
        public float line_of_sight_test_offset;

        [FieldOffset(352)]
        public float line_of_sight_test_offset_towards_user;

        [FieldOffset(356)]
        public int deploy_animation; //stringid

        [FieldOffset(360)]
        public float deploy_duration_override;

        [FieldOffset(364)]
        public float max_range;

        [FieldOffset(368)]
        public int biped_attach_marker; //stringid

        [FieldOffset(372)]
        public RealVector3D biped_attach_offset;

        [FieldOffset(384)]
        public int pull_animation_airborne; //stringid

        [FieldOffset(388)]
        public int pull_animation_grounded; //stringid

        [FieldOffset(392)]
        public float pull_time_to_detach_allowed;

        [FieldOffset(396)]
        public float pull_detach_damage_threshold;

        [FieldOffset(400)]
        public float pull_stuck_timeout;

        [FieldOffset(404)]
        public float pull_min_progress_per_update;

        [FieldOffset(408)]
        public float pull_post_crouch_suppression_wait_time;

        [FieldOffset(412)]
        public float pull_collision_rejection_post_completion_duration;

        [FieldOffset(416)]
        public float pull_collision_rejection_threshold_target_velocity;

        [FieldOffset(420)]
        public float pull_collision_damage_immunity_delay;

        [FieldOffset(424)]
        public float pull_collision_damage_imminuty_minimum_velocity;

        [FieldOffset(428)]
        public float pull_collision_damage_immunity_post_completion_duration;

        [FieldOffset(432)]
        public RealBounds pull_close_range_definition;

        [FieldOffset(440)]
        public float pull_physics_pill_offset_scalar_grounded;

        [FieldOffset(444)]
        public float pull_physics_pill_offset_scalar_airborne;

        [FieldOffset(448)]
        public float pull_launch_vertical_impulse;

        [FieldOffset(452)]
        public float pull_launch_aim_impulse;

        [FieldOffset(456)]
        public float pull_launch_decay_duration;

        [FieldOffset(460)]
        public float pull_acceleration_phase_duration;

        [FieldOffset(464)]
        public float pull_base_target_velocity;

        [FieldOffset(468)]
        public float pull_exit_max_velocity;

        [FieldOffset(472)]
        public DataReferenceField base_pull_acceleration_function;

        [FieldOffset(496)]
        public DataReferenceField launch_impulse_decay_function;

        [FieldOffset(520)]
        public float pull_target_velocity_filter_fraction;

        [FieldOffset(524)]
        public float pull_aim_vector_influence_level_geo;

        [FieldOffset(528)]
        public float pull_aim_vector_influence_bipeds;

        [FieldOffset(532)]
        public float pull_aim_vector_influence_vehicles;

        [FieldOffset(536)]
        public float pull_aim_vector_influence_falloff_inner_angle; //angle

        [FieldOffset(540)]
        public float pull_aim_vector_influence_falloff_outer_angle; //angle

        [FieldOffset(544)]
        public float pull_aim_vector_influence_accumulation_rate;

        [FieldOffset(548)]
        public float aim_vector_influence_accumulation_starting_scalar;

        [FieldOffset(552)]
        public DataReferenceField pull_aim_vector_influence_falloff_function;

        [FieldOffset(576)]
        public DataReferenceField pull_aim_vector_influence_scale_function;

        [FieldOffset(600)]
        public float pull_air_control_scalar_level_geo;

        [FieldOffset(604)]
        public float pull_air_control_scalar_forward_and_back_level_geo;

        [FieldOffset(680)]
        public float pull_air_control_scalar_left_and_right_level_geo;

        [FieldOffset(612)]
        public float pull_air_control_scalar_bipeds;

        [FieldOffset(616)]
        public float pull_air_control_scalar_forward_and_back_bipeds;

        [FieldOffset(620)]
        public float pull_air_control_scalar_left_and_right_bipeds;

        [FieldOffset(624)]
        public float pull_air_control_scalar_vehicles;

        [FieldOffset(628)]
        public float pull_air_control_scalar_forward_and_back_vehicles;

        [FieldOffset(632)]
        public float pull_air_control_scalar_left_and_right_vehicles;

        [FieldOffset(636)]
        public float pull_air_control_accumulation_rate;

        [FieldOffset(640)]
        public float pull_air_control_accumulation_starting_scalar;

        [FieldOffset(644)]
        public float pull_completion_distance_level_geo;

        [FieldOffset(648)]
        public float pull_completion_distance_level_geo_ceilings;

        [FieldOffset(652)]
        public float pull_completion_distance_level_geo_floors;

        [FieldOffset(656)]
        public float pull_completion_distance_bipeds;

        [FieldOffset(660)]
        public float pull_completion_distance_vehicles;

        [FieldOffset(664)]
        public float pull_vehicle_interaction_distance;

        [FieldOffset(668)]
        public float pull_vehicle_interation_time;

        [FieldOffset(672)]
        public float pull_biped_automlee_max_angle; //angle

        [FieldOffset(676)]
        public float max_allowed_angle_deviation_ceiling; //angle

        [FieldOffset(680)]
        public float max_allowed_angle_deviation_floor; //angle

        [FieldOffset(684)]
        public float pull_line_of_sight_break_time;

        [FieldOffset(688)]
        public float pull_aftermath_duration;

        [FieldOffset(692)]
        public float pull_aftermath_velocity_peak;

        [FieldOffset(696)]
        public DataReferenceField pull_aftermath_velocity_scale_function;

        [FieldOffset(720)]
        public int pull_aftermath_animation_name; //stringid

        [FieldOffset(724)]
        public TagReference pull_aftermath_impact_effect;

        [FieldOffset(752)]
        public float reel_in_acceleration_phase_duration;

        [FieldOffset(756)]
        public float reel_in_base_target_velocity;

        [FieldOffset(760)]
        public float reel_in_launch_grounded_slope; //angle

        [FieldOffset(764)]
        public float reel_in_launch_vertical_impulse_duration;

        [FieldOffset(768)]
        public float reel_in_launch_vertical_impulse_grounded;

        [FieldOffset(772)]
        public float reel_in_launch_vertical_impulse_airborne;

        [FieldOffset(776)]
        public RealBounds reel_in_launch_roll_impulse_airborne;

        [FieldOffset(784)]
        public RealBounds reel_in_launch_roll_rotation_impulse_grounded;

        [FieldOffset(792)]
        public RealBounds reel_in_launch_pitch_impulse_airborne;

        [FieldOffset(800)]
        public RealBounds reel_in_launch_pitch_rotation_impulse_grounded;

        [FieldOffset(808)]
        public float reel_in_exit_velocity;

        [FieldOffset(812)]
        public float reel_in_grab_distance;

        [FieldOffset(816)]
        public float reel_in_grab_vertical_offset;

        [FieldOffset(820)]
        public float reel_in_time_to_detach_allowed;

        [FieldOffset(824)]
        public float reel_in_detach_damage_threshold;

        [FieldOffset(828)]
        public float reel_in_line_of_sight_break_time;

        [FieldOffset(832)]
        public float reel_in_min_acceleration_percentage_to_break;

        [FieldOffset(836)]
        public RealBounds reel_in_close_range_definition;

        [FieldOffset(844)]
        public int reel_in_animation; //stringid

        [FieldOffset(848)]
        public DataReferenceField reel_in_acceleration_function;

        [FieldOffset(872)]
        public DataReferenceField reel_in_tangential_velocity_damping_function;

        [FieldOffset(896)]
        public float bash_activation_input_hold_time;

        [FieldOffset(900)]
        public float bash_activation_max_aim_angle; //angle

        [FieldOffset(904)]
        public float bash_max_aim_lock_rate_of_change;

        [FieldOffset(908)]
        public float bash_peak_velocity;

        [FieldOffset(912)]
        public float bash_charge_up_duration;

        [FieldOffset(916)]
        public DataReferenceField bash_charge_up_velocity_scale_function;

        [FieldOffset(940)]
        public float bash_acceleration_duration;

        [FieldOffset(944)]
        public DataReferenceField bash_acceleration_velocity_scale_function;

        [FieldOffset(968)]
        public float bash_stuck_timeout;

        [FieldOffset(972)]
        public float bash_aftermath_duration;

        [FieldOffset(976)]
        public float bash_aftermath_recoil_peak_velocity;

        [FieldOffset(980)]
        public DataReferenceField bash_recoil_velocity_scale_function;

        [FieldOffset(1004)]
        public int bash_charge_up_animation; //stringid

        [FieldOffset(1008)]
        public int bash_flight_animation; //stringid

        [FieldOffset(1012)]
        public int bash_finish_grounded_animation; //stringid

        [FieldOffset(1016)]
        public int bash_finish_airborne_animation; //stringid

        [FieldOffset(1020)]
        public float bash_finish_animation_vertical_angle_tolerance; //angle

        [FieldOffset(1024)]
        public TagReference bash_charge_up_effect;

        [FieldOffset(1052)]
        public TagReference bash_launch_effect;

        [FieldOffset(1080)]
        public TagReference bash_active_effect;

        [FieldOffset(1108)]
        public TagReference bash_completion_effect;

        [FieldOffset(1136)]
        public TagReference bash_owner_damage;

        [FieldOffset(1164)]
        public TagReference bash_pound_aoe_damage;

        [FieldOffset(1192)]
        public float bash_pound_aoe_epicenter_walkback_distance;

        [FieldOffset(1196)]
        public int retract_animation; //stringid

        [FieldOffset(1200)]
        public float exit_animation_speed_scalar;

        [FieldOffset(1204)]
        public float miss_cooldown_delay;

        [FieldOffset(1208)]
        public float aim_assist_range;

        [FieldOffset(1212)]
        public float aim_assist_cone_angle_at_projectile_launch; //angle

        [FieldOffset(1216)]
        public float aim_assist_cone_angle_while_projectile_flying; //angle

        [FieldOffset(1220)]
        public float aim_assist_weight_angle;

        [FieldOffset(1224)]
        public float aim_assist_weight_distance;

        [FieldOffset(1228)]
        public float aim_assist_weight_biped;

        [FieldOffset(1232)]
        public float aim_assist_weight_vehicle;

        [FieldOffset(1236)]
        public float aim_assist_weight_weapon;
    }
}
