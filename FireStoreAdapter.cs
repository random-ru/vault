using System;
using Google.Cloud.Firestore;

namespace vault;


public class FireStoreAdapter : IFireStoreAdapter
{
    private FirestoreDb db { get; }
    public FireStoreAdapter() => db = 
        FirestoreDb.Create(Environment.GetEnvironmentVariable("FIRESTORE_PROJECT_ID"));
    public CollectionReference Namespaces 
        => db.Collection("namespaces");
}

public interface IFireStoreAdapter
{
    CollectionReference Namespaces { get; }
}