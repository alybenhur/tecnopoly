using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class QuestionCard
{
    public string questionText;              // El texto de la pregunta
    public List<string> answerOptions;       // Lista de opciones de respuesta
    public int correctAnswerIndex;           // �ndice de la respuesta correcta (0-basado)
    public int rewardCredits = 100;          // Cr�ditos que gana el jugador si responde correctamente
    public int penaltyCredits = 50;          // Cr�ditos que pierde el jugador si responde incorrectamente
    public string correctExplanation;        // Explicaci�n opcional para mostrar cuando la respuesta es correcta
    public string incorrectExplanation;      // Explicaci�n opcional para mostrar cuando la respuesta es incorrecta
    public Sprite questionImage;             // Imagen opcional para acompa�ar la pregunta
}