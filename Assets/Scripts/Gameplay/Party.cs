public struct PartyAssignment
{
    public Unit AssignedMember;
    public Skill AssignedSkill;
}

public enum PartyPosition
{
    NotAssigned = -1,
    Defense = 0,
    Attack = 1,
    Support = 2,
}

public class Party {

    public readonly int MaxPartySize = 3;
    public readonly int MaxAssignments = 3;

    public Unit[] Members;
    public PartyAssignment[] Assignment;

    public Party()
    {
        Members = new Unit[MaxPartySize];
        Assignment = new PartyAssignment[MaxAssignments];
    }

    public int GetMemberCount()
    {
        for (int i = 0; i < Members.Length; ++i)
        {
            if (Members[i] == null)
            {
                return i;
            }
        }
        return Members.Length;
    }
    
    public bool IsPartyMember(Unit checkUnit)
    {
        // Special case for passing in a null pointer.
        // A null unit can't be a party member of any party.
        if (checkUnit == null)
        {
            return false;
        }

        return checkUnit.JoinedParty == this;
    }

    public bool AddPartyMember(Unit newMember)
    {
        // If the unit is already a member of any party, it can't join.
        // Caller should make sure the unit leaves its previous party before joining.
        if (newMember == null || newMember.JoinedParty != null)
        {
            return false;
        }

        int memberCount = GetMemberCount();
        if (memberCount >= MaxPartySize)
        {
            return false;
        }

        Members[memberCount] = newMember;
        newMember.JoinedParty = this;
        return true;
    }

    public bool RemovePartyMember(Unit removedMember)
    {
        if (!IsPartyMember(removedMember))
        {
            return false;
        }

        RemoveAssignment(removedMember);
        for (int i = 0; i < Members.Length; ++i)
        {
            if (Members[i] == removedMember)
            {
                // Swap and remove.
                int swapPosition = GetMemberCount() - 1;
                Members[i] = Members[swapPosition];
                Members[swapPosition] = null;
                break;
            }
        }
        removedMember.JoinedParty = null;
        return true;
    }

    public Unit GetAssignedUnit(PartyPosition position)
    {
        if (position == PartyPosition.NotAssigned)
        {
            return null;
        }
        int positionIndex = (int)position;
        return Assignment[positionIndex].AssignedMember;
    }

    public Skill GetAssignedSkill(PartyPosition position)
    {
        if (position == PartyPosition.NotAssigned)
        {
            return null;
        }
        int positionIndex = (int)position;
        return Assignment[positionIndex].AssignedSkill;
    }

    public PartyPosition GetAssignedPosition(Unit member)
    {
        for (int i = 0; i < MaxAssignments; ++i)
        {
            if (Assignment[i].AssignedMember == member)
            {
                return (PartyPosition)i;
            }
        }
        return PartyPosition.NotAssigned;
    }

    // Find a usable skill when a member is assigned to a specified position.
    public Skill GetAssignableSkill(Unit member, PartyPosition position)
    {
        foreach (Skill skill in member.Skills)
        {
            if (skill.CanBeUsedInPosition(position))
            {
                return skill;
            }
        }
        return null;
    }

    public bool SetAssignment(Unit member, PartyPosition position)
    {
        // We support unassigning a member unit by assigning it to PartyPosition.NotAssigned.
        if (position == PartyPosition.NotAssigned)
        {
            return RemoveAssignment(member);
        }

        // Of course the member should be in the party.
        if (!IsPartyMember(member))
        {
            return false;
        }

        // The target position must be unassigned first.
        // The caller should make sure that this happens before SetAssignment is called.
        int positionIndex = (int)position;
        if (Assignment[positionIndex].AssignedMember != null)
        {
            return false;
        }

        Assignment[positionIndex].AssignedMember = member;
        Assignment[positionIndex].AssignedSkill = GetAssignableSkill(member, position);

        return true;
    }

    public bool SetAssignment(Unit member, PartyPosition position, Skill skill)
    {
        // We support unassigning a member unit by assigning it to PartyPosition.NotAssigned.
        if (position == PartyPosition.NotAssigned)
        {
            return RemoveAssignment(member);
        }

        // Of course the member should be in the party.
        if (!IsPartyMember(member))
        {
            return false;
        }

        // Additionally, make sure that the unit has the skill and that the skill is valid.
        if (!member.HasSkill(skill) || !skill.CanBeUsedInPosition(position))
        {
            return false;
        }

        // The target position must be unassigned first.
        // The caller should make sure that this happens before SetAssignment is called.
        int positionIndex = (int)position;
        if (Assignment[positionIndex].AssignedMember != null)
        {
            return false;
        }

        Assignment[positionIndex].AssignedMember = member;
        Assignment[positionIndex].AssignedSkill = skill;

        return true;
    }

    public bool RemoveAssignment(Unit member)
    {
        for (int i = 0; i < MaxAssignments; ++i)
        {
            if (Assignment[i].AssignedMember == member)
            {
                Assignment[i].AssignedMember = null;
                Assignment[i].AssignedSkill = null;
            }
        }

        return true;
    }
}
