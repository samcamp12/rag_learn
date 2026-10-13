import { Markdown } from './Markdown';
import type { Lesson } from '../lessons';
import { projectMap } from '../lessons';

export type RightTab = 'instructions' | 'docs' | 'map';

type Props = {
  lesson: Lesson;
  tab: RightTab;
  onTabChange: (tab: RightTab) => void;
};

const tabs: { id: RightTab; label: string }[] = [
  { id: 'instructions', label: 'Instructions' },
  { id: 'docs', label: 'Docs' },
  { id: 'map', label: 'Project map' },
];

export function RightPanel({ lesson, tab, onTabChange }: Props) {
  const content =
    tab === 'instructions' ? lesson.instructions : tab === 'docs' ? lesson.docs : projectMap;

  return (
    <aside className="sidebar right">
      <div className="tabs">
        {tabs.map((t) => (
          <button
            key={t.id}
            className={t.id === tab ? 'tab active' : 'tab'}
            onClick={() => onTabChange(t.id)}
          >
            {t.label}
          </button>
        ))}
      </div>
      <div className="right-body">
        <Markdown source={content} />
      </div>
    </aside>
  );
}
