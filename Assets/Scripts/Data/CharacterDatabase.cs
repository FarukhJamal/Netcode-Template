using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Data;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "New Character Database",menuName = "Characters/Database")]
public class CharacterDatabase : ScriptableObject
{
    [SerializeField] private Character[] characters=Array.Empty<Character>();

    public Character[] GetAllCharacters() => characters;

    public Character GetCharacterById(int id)
    {
        foreach (Character character in characters)
        {
            if (character.Id == id)
                return character;
        }

        return null;
    }

    public bool IsValidCharacterId(int id)
    {
        return characters.Any(x => x.Id == id);
    }
}
