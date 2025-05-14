using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class QuestionCard
{
    public string questionText;              // El texto de la pregunta
    public List<string> answerOptions;       // Lista de opciones de respuesta
    public int correctAnswerIndex;           // Índice de la respuesta correcta (0-basado)
    public int rewardCredits = 100;          // Créditos que gana el jugador si responde correctamente
    public int penaltyCredits = 50;          // Créditos que pierde el jugador si responde incorrectamente
    public string correctExplanation;        // Explicación opcional para mostrar cuando la respuesta es correcta
    public string incorrectExplanation;      // Explicación opcional para mostrar cuando la respuesta es incorrecta
    public Sprite questionImage;             // Imagen opcional para acompañar la pregunta
}