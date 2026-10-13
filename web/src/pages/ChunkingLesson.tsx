import { useEffect, useState } from 'react';
import { api, type ChunkDto, type DocumentSummary } from '../api';

export function ChunkingLesson() {
  const [documents, setDocuments] = useState<DocumentSummary[]>([]);
  const [selectedId, setSelectedId] = useState<number | null>(null);
  const [chunks, setChunks] = useState<ChunkDto[]>([]);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    api.documents().then((docs) => {
      setDocuments(docs);
      if (docs.length > 0) setSelectedId(docs[0].id);
    }).catch((e: Error) => setError(e.message));
  }, []);

  useEffect(() => {
    if (selectedId === null) return;
    api.chunks(selectedId).then(setChunks).catch((e: Error) => setError(e.message));
  }, [selectedId]);

  if (documents.length === 0) {
    return <p className="muted">No documents yet. Ingest them in lesson 2 first.</p>;
  }

  return (
    <section>
      {error && <p className="error">{error}</p>}
      <div className="card">
        <label>
          Document{' '}
          <select value={selectedId ?? ''} onChange={(e) => setSelectedId(Number(e.target.value))}>
            {documents.map((d) => (
              <option key={d.id} value={d.id}>{d.fileName}</option>
            ))}
          </select>
        </label>
        <p className="muted">{chunks.length} chunk(s). Each card is one row in the Chunks table.</p>
      </div>

      {chunks.map((c) => (
        <div className="card chunk" key={c.id}>
          <div className="card-header">
            <strong>#{c.position}</strong>
            <span className="muted">{c.heading ?? '(no heading)'}</span>
            <span className="badge">{c.charCount} chars</span>
          </div>
          <pre>{c.content}</pre>
        </div>
      ))}
    </section>
  );
}
