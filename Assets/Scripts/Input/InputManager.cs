using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Users;

public static class InputManager
{
    #region Key bindings
    public enum InputCommand
    {
        Any,
        Interact,
        Jump,
        Sprint,
        PickUp
    }

    private static Dictionary<InputCommand, Key> _keyboardMappings = new Dictionary<InputCommand, Key>() 
    {
        { InputCommand.Interact, Key.E },
        { InputCommand.Jump, Key.Space },
        { InputCommand.Sprint, Key.LeftShift },
    };

    private static Dictionary<InputCommand, MouseButton> _mouseMappings = new Dictionary<InputCommand, MouseButton>()
    {
        {InputCommand.PickUp, MouseButton.Left }
    };

    private static Dictionary<InputCommand, GamepadButton> _gamepadMappings = new Dictionary<InputCommand, GamepadButton>()
    {
        { InputCommand.Interact, GamepadButton.West },
        { InputCommand.Jump, GamepadButton.South },
        { InputCommand.Sprint, GamepadButton.LeftStick },
        { InputCommand.PickUp, GamepadButton.West }
    };

    public static Dictionary<InputCommand, Key> KeyboardMappings
    {
        get { return _keyboardMappings; }
    }

    public static Dictionary<InputCommand, MouseButton> MouseMapping
    {
        get { return _mouseMappings; }
    }

    public static Dictionary<InputCommand, GamepadButton> GamepadMapping
    {
        get { return _gamepadMappings; }
    }

    #endregion Key bindings

    // Could extend this later to include other gamepad types
    private enum InputType
    {
        KeyboardAndMouseInput,
        GamepadInput
    }
    private static InputType _lastInputType = InputType.KeyboardAndMouseInput;

    public static bool IsGamepadEnabled()
    {
        return Gamepad.all.Count > 0;
    }

    public static bool IsUsingGamepad()
    {
        return _lastInputType == InputType.GamepadInput;
    }

    public static Vector2 GetMovementAxis()
    {
        Vector2 movement = Vector2.zero;
        if (Keyboard.current != null)
        {
            var left = Keyboard.current.aKey.isPressed ? 1 : 0;
            var right = Keyboard.current.dKey.isPressed ? 1 : 0;
            var up = Keyboard.current.wKey.isPressed ? 1 : 0;
            var down = Keyboard.current.sKey.isPressed ? 1 : 0;

            var keyboardMovement = new Vector2(right - left, up - down);

            if(keyboardMovement != Vector2.zero) _lastInputType = InputType.KeyboardAndMouseInput;

            movement += keyboardMovement;
        }
        if (Gamepad.current != null)
        {
            var gamepadMovement = Gamepad.current.leftStick.ReadValue();
            if (gamepadMovement != Vector2.zero) _lastInputType = InputType.GamepadInput;
            movement += gamepadMovement;
        }

        return movement.normalized;
    }

    public static Vector2 GetLookAxis()
    {
        Vector2 movement = Vector2.zero;
        if(Mouse.current != null)
        {
            var keyboardMovement = new Vector2(Mouse.current.delta.x.ReadValue(), Mouse.current.delta.y.ReadValue());

            if(keyboardMovement != Vector2.zero) _lastInputType = InputType.KeyboardAndMouseInput;
            movement += keyboardMovement;
        }
        if(Gamepad.current != null)
        {
            var gamepadMovement = Gamepad.current.rightStick.ReadValue();
            if(gamepadMovement != Vector2.zero) _lastInputType = InputType.GamepadInput;
            movement += gamepadMovement;
        }

        return movement;
    }

    #region Pressed

    private static bool IsAnyKeyPressed()
    {
        if (Keyboard.current != null && Keyboard.current.anyKey.IsPressed())
            return true;

        if (Gamepad.current != null)
        {
            foreach (var control in Gamepad.current.allControls)
            {
                if (control.IsPressed())
                {
                    return true;
                }
            }
        }

        if (Mouse.current != null)
        {
            foreach (var control in Mouse.current.allControls)
            {
                if (control.IsPressed())
                {
                    return true;
                }
            }
        }

        return false;
    }

    public static bool IsCommandPressed(InputCommand command)
    {
        if (command == InputCommand.Any)
        {
            return IsAnyKeyPressed();
        }

        if (_gamepadMappings.ContainsKey(command))
        {
            var button = _gamepadMappings[command];
            if (Gamepad.current != null && Gamepad.current[button].IsPressed()) return true;
        }

        if (_keyboardMappings.ContainsKey(command))
        {
            var key = _keyboardMappings[command];
            if (Keyboard.current != null && Keyboard.current[key].IsPressed()) return true;
        }

        if (_mouseMappings.ContainsKey(command))
        {
            var button = _mouseMappings[command];
            if (Mouse.current != null && Mouse.current[button.ToString().ToLower() + "Button"].IsPressed()) return true;
        }

        return false;
    }

    #endregion Pressed

    #region Just pressed

    private static bool IsAnyKeyJustPressed()
    {
        if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
            return true;

        if (Gamepad.current != null)
        {
            foreach (ButtonControl control in Gamepad.current.allControls)
            {
                if (control.wasPressedThisFrame)
                {
                    return true;
                }
            }
        }

        if (Mouse.current != null)
        {
            foreach (ButtonControl control in Mouse.current.allControls)
            {
                if (control.wasPressedThisFrame)
                {
                    return true;
                }
            }
        }

        return false;
    }

    public static bool IsCommandJustPressed(InputCommand command)
    {
        if (command == InputCommand.Any)
        {
            return IsAnyKeyJustPressed();
        }

        if (_gamepadMappings.ContainsKey(command))
        {
            var button = _gamepadMappings[command];
            if (Gamepad.current != null && Gamepad.current[button].wasPressedThisFrame) return true;
        }

        if (_keyboardMappings.ContainsKey(command))
        {
            var key =_keyboardMappings[command];
            if (Keyboard.current != null && Keyboard.current[key].wasPressedThisFrame) return true;
        }

        if (_mouseMappings.ContainsKey(command))
        {
           var button = _mouseMappings[command];
            if (Mouse.current != null && (Mouse.current[button.ToString().ToLower() + "Button"] as ButtonControl).wasPressedThisFrame) return true;
        }

        return false;
    }

    #endregion Just pressed

    #region Just released

    private static bool IsAnyKeyJustReleased()
    {
        if (Keyboard.current != null && Keyboard.current.anyKey.wasReleasedThisFrame)
            return true;

        if (Gamepad.current != null)
        {
            foreach (ButtonControl control in Gamepad.current.allControls)
            {
                if (control.wasReleasedThisFrame)
                {
                    return true;
                }
            }
        }

        if (Mouse.current != null)
        {
            foreach (ButtonControl control in Mouse.current.allControls)
            {
                if (control.wasReleasedThisFrame)
                {
                    return true;
                }
            }
        }

        return false;
    }

    public static bool IsCommandJustReleased(InputCommand command)
    {
        if (command == InputCommand.Any)
        {
            return IsAnyKeyJustReleased();
        }

        if (_gamepadMappings.ContainsKey(command))
        {
            var button = _gamepadMappings[command];
            if (Gamepad.current != null && Gamepad.current[button].wasReleasedThisFrame) return true;
        }

        if (_keyboardMappings.ContainsKey(command))
        {
            var key = _keyboardMappings[command];
            if (Keyboard.current != null && Keyboard.current[key].wasReleasedThisFrame) return true;
        }

        if (_mouseMappings.ContainsKey(command))
        {
            var button = _mouseMappings[command];
            if (Mouse.current != null && (Mouse.current[button.ToString().ToLower() + "Button"] as ButtonControl).wasReleasedThisFrame) return true;
        }

        return false;
    }

    #endregion Just released
}
