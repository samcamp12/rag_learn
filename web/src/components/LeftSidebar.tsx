import { useEffect, useState } from 'react';
import { api } from '../api';
import { lessons, upcomingLessons, type Lesson, type LessonId } from '../lessons';

type Props = {
  activeId: LessonId;
  onSelect: (id: LessonId) => void;
};

export function LeftSidebar({ activeId, onSelect }: Props) {
  const [backendOnline, setBackendOnline] = useState<boolean | null>(null);

  useEffect(() => {
    api.health().then(() => setBackendOnline(true)).catch(() => setBackendOnline(false));
  }, []);

  return (
    <aside className="sidebar left">
      <h1 className="brand">RAG Learn</h1>
      <p className="muted small">Learn RAG with C# and SQLite</p>

      <nav>
        <h2 className="section-title">Lessons</h2>
        <ul>
          {lessons.map((lesson: Lesson) => (
            <li key={lesson.id}>
              <button
                className={lesson.id === activeId ? 'nav-item active' : 'nav-item'}
                onClick={() => onSelect(lesson.id)}
              >
                {lesson.title}
              </button>
            </li>
          ))}
          {upcomingLessons.map((title) => (
            <li key={title}>
              <span className="nav-item disabled">{title} (coming next)</span>
            </li>
          ))}
        </ul>
      </nav>

      <div className="backend-status">
        <span className={`dot ${backendOnline === null ? 'pending' : backendOnline ? 'ok' : 'bad'}`} />
        {backendOnline === null && 'Checking backend...'}
        {backendOnline === true && 'Backend online (port 5022)'}
        {backendOnline === false && 'Backend offline. Start the API first.'}
      </div>
    </aside>
  );
}
