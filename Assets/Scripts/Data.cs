using System;
using System.Collections.Generic;
using Unity.VisualScripting;

[Serializable]
public class Name
{
    public List<string> name;
}

[Serializable]
public class Data
{
    public short[] nameNstate;
    public string name;
    public string dialog;
}

[Serializable]
public class Question
{
    public List<string> solutions;
    public List<short> answers;
}

[Serializable]
public class BindQA
{
    public List<string> q;
    public List<string> anslist;
    public Question ans;
    public List<Name> dialog;
}

[Serializable]
public class Reaction
{
    public List<string> reactions;
}

[Serializable]
public class DataLoad
{
    public List<string> talkname;
    public List<Data> data;

    public void Load(List<string> names, List<string> dialogs, ref List<string> talk, List<short[]> nns)
    {
        talk = talkname;
        for(int i = 0; i < data.Count; ++i)
        {
            names.Add(data[i].name);
            dialogs.Add(data[i].dialog);
            nns.Add(data[i].nameNstate);
        }
    }
}

[Serializable]
public class QuestionLoad
{
    public List<string> talkname;
    public List<string> qdata;
    public List<Question> sdata;
    public List<Name> dialog;

    public BindQA Load(ref List<string> talk)
    {
        talk = talkname;
        BindQA qa = new BindQA();
        qa.q = qdata;
        qa.ans = sdata[UnityEngine.Random.Range(0, sdata.Count)];
        qa.anslist = new List<string>();
        for (int i = 0; i < sdata.Count; ++i)
            qa.anslist.Add(sdata[i].solutions[0]);
        if (dialog != null)
            qa.dialog = dialog;
        return qa;
    }
}