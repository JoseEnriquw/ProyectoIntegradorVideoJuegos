using UnityEngine;

namespace FuncionalidadesCore
{
    // =============================================
    // INTERFACES DE INTERACCIÓN
    // Definen contratos para distintos tipos de interacción con objetos del juego.
    // =============================================

    /// <summary>Interacción de inicio (click/press).</summary>
    public interface IInteractStart
    {
        void InteractStart();
        bool CanInteract();
    }

    /// <summary>Interacción de mantener presionado.</summary>
    public interface IInteractHold
    {
        void InteractHold(Vector3 point);
    }

    /// <summary>Interacción temporizada (barra de progreso).</summary>
    public interface IInteractTimed
    {
        float InteractTime { get; }
        void InteractStartTimed();
    }

    /// <summary>Interacción de detener/soltar.</summary>
    public interface IInteractStop
    {
        void InteractStop();
    }

    /// <summary>Información de hover sobre un objeto interactable.</summary>
    public interface IInteractInfo
    {
        string InteractTitle { get; }
    }

    /// <summary>Para objetos que cambian de estado al interactuar.</summary>
    public interface IInteractStates
    {
        StateParams OnInteract();
    }

    /// <summary>Parámetros de estado de interacción.</summary>
    public struct StateParams
    {
        public string stateKey;
        public int stateValue;

        public StateParams(string key, int value)
        {
            stateKey = key;
            stateValue = value;
        }
    }

    // =============================================
    // INTERFACES DE EXAMINACIÓN
    // =============================================

    /// <summary>Examinar objeto con click.</summary>
    public interface IExamineClick
    {
        void OnExamineClick(Vector3 hitPoint);
    }

    /// <summary>Examinar objeto con arrastre vertical.</summary>
    public interface IExamineDragVertical
    {
        void OnExamineDragVertical(float dragDelta);
    }

    /// <summary>Examinar objeto con arrastre horizontal.</summary>
    public interface IExamineDragHorizontal
    {
        void OnExamineDragHorizontal(float dragDelta);
    }

    // =============================================
    // INTERFACES DE ARRASTRE (DRAG)
    // =============================================

    /// <summary>Evento al iniciar arrastre.</summary>
    public interface IOnDragStart
    {
        void OnDragStart();
    }

    /// <summary>Evento al finalizar arrastre.</summary>
    public interface IOnDragEnd
    {
        void OnDragEnd();
    }

    /// <summary>Evento durante el arrastre (cada frame).</summary>
    public interface IOnDragUpdate
    {
        void OnDragUpdate(Vector3 dragDelta);
    }

    // =============================================
    // INTERFACES DE CHARACTER CONTROLLER
    // =============================================

    /// <summary>Para superficies que reaccionan al CharacterController del jugador.</summary>
    public interface ICharacterControllerHit
    {
        void OnCharacterControllerEnter(CharacterController controller);
        void OnCharacterControllerExit();
    }
}
